using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsumerDLX
{

    class ConsumerDLX
    {
        static void Main(string[] args)
        {
            //var factory = new ConnectionFactory { HostName = "gkfoltgr:6NIVZuG5hhQtO65_wD5Yvtioy0SK3Wr3@buffalo.rmq.cloudamqp.com/gkfoltgr" };
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://gkfoltgr:6NIVZuG5hhQtO65_wD5Yvtioy0SK3Wr3@buffalo.rmq.cloudamqp.com/gkfoltgr"),
                //HostName = "buffalo.rmq.cloudamqp.com",
                //Port = 1883,
                //UserName = "gkfoltgr:gkfoltgr",
                //Password = "6NIVZuG5hhQtO65_wD5Yvtioy0SK3Wr3"
            };

            using var connection = factory.CreateConnection();
            using (var channel = connection.CreateModel())
            {
                //const string ExchangeName = "DeadLetterExchange";
                //channel.ExchangeDeclare(exchange: ExchangeName,
                //                        type: ExchangeType.Fanout);

                // Fila 1 - Fila DLX
                const string DeadLetterQueueName = "Dead_Letter_Queue";
                channel.QueueDeclare(queue: DeadLetterQueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                //channel.QueueBind(queue: DeadLetterQueueName,
                //                  exchange: ExchangeName,
                //                  string.Empty);

                // Argumento da regra do DLX e Exchange
                //var arguments = new Dictionary<string, object>
                //{
                //    { "x-dead-letter-exchange", ExchangeName }
                //};
                // Fila 2 (original) - Fila normal - Usando novo argumento
                //const string BasicQueueName = "task_queue";
                //channel.QueueDeclare(queue: BasicQueueName,
                //                     durable: true,
                //                     exclusive: false,
                //                     autoDelete: false,
                //                     arguments: arguments);

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

                        //var total = int.Parse(message);

                        Console.WriteLine($" [*] Received {message}.");

                        eventArgs.BasicProperties.Headers.TryGetValue("x-death", out object xDeath);

                        eventArgs.Redelivered = true;

                        var xDeath2 = ((List<object>)xDeath)[0];
                        long count = (long)((Dictionary<string, object>)xDeath2)["count"];

                        var xDeath3 = ((List<object>)xDeath);
                        if (count > 3)
                        {
                            Console.WriteLine("Erro.");
                        }

                        string count2 = count.ToString();
                        if (!long.TryParse(count2, out long count3) || count3 > 5)
                        {
                            Console.WriteLine("Erro.");
                        }

                        //if (!eventArgs.BasicProperties.Headers.ContainsKey("x-death"))
                        //    return PostExceptionAckStrategy.ShouldNackWithoutRequeue;

                        //var q = eventArgs.BasicProperties.Headers["x-death"];
                        //var w = (List)q;

                        //if (w == null || w.Count < 3)
                        //    return PostExceptionAckStrategy.ShouldNackWithoutRequeue;

                        //return base.HandleConsumerError(context, exception);

                        RequeueMessage(message, eventArgs.BasicProperties.Headers);

                        channel.BasicAck(deliveryTag: eventArgs.DeliveryTag,
                                         multiple: false);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);

                        channel.BasicNack(deliveryTag: eventArgs.DeliveryTag,
                                          multiple: false,
                                          requeue: true);
                    }
                };

                channel.BasicConsume(queue: DeadLetterQueueName,
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private static void RequeueMessage(string message, IDictionary<string, object> headers)
        {
            //var factory = new ConnectionFactory { HostName = "localhost" };
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://gkfoltgr:6NIVZuG5hhQtO65_wD5Yvtioy0SK3Wr3@buffalo.rmq.cloudamqp.com/gkfoltgr"),
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                const string ExchangeName = "DeadLetterExchange";
                var arguments = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", ExchangeName }
                };



                const string QueueName = "task_queue";
                channel.QueueDeclare(queue: QueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: arguments);

                var basicProperties = channel.CreateBasicProperties();
                basicProperties.Headers = headers;

                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: string.Empty,
                                     routingKey: QueueName,
                                     basicProperties: basicProperties,
                                     body: body);

                Console.WriteLine($" [x] Sent {message}");
            }
        }
    }
}
