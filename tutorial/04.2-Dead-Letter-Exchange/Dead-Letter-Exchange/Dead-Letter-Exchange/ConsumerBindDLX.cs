using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeadLetterExchange
{
    class ConsumerBindDLX
    {
		static void Main(string[] args)
		{
			var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = factory.CreateConnection();
            using (var channel = connection.CreateModel())
            {
                const string ExchangeName = "DeadLetterExchange";
                channel.ExchangeDeclare(exchange: ExchangeName,
                                        type: ExchangeType.Fanout);

                // Fila 1 - Fila DLX
                const string DeadLetterQueueName = "Dead_Letter_Queue";
                channel.QueueDeclare(queue: DeadLetterQueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                channel.QueueBind(queue: DeadLetterQueueName,
                                  exchange: ExchangeName,
                                  string.Empty);

                // Argumento da regra do DLX e Exchange
                var arguments = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", ExchangeName }
                };
                // Fila 2 (original) - Fila normal - Usando novo argumento
                const string BasicQueueName = "task_queue";
                channel.QueueDeclare(queue: BasicQueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: arguments);

                channel.BasicQos(prefetchSize: 0,
                                 prefetchCount: 1,
                                 global: false);

                Console.WriteLine(" [*] Waiting for message.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, eventArgs) =>
                {
                    try
                    {
                        var body = eventArgs.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        var total = int.Parse(message);

                        Console.WriteLine($" [*] Received {message} {total}");

                        channel.BasicAck(deliveryTag: eventArgs.DeliveryTag,
                                         multiple: false);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);

                        channel.BasicNack(deliveryTag: eventArgs.DeliveryTag,
                                          multiple: false, 
                                          requeue: false);
                    }
                };

                channel.BasicConsume(queue: BasicQueueName,
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
	}
}
