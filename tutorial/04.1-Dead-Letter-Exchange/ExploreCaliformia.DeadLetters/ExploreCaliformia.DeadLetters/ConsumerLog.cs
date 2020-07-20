using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ExploreCaliformia.DeadLetters
{
    class ConsumerLog
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var exchangeName = "DLX";

                    channel.ExchangeDeclare(exchange: exchangeName, 
                                            type: ExchangeType.Direct,
                                            durable: true);
                    channel.QueueDeclare(queue: "deadLetters",
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false);
                    channel.QueueBind(queue: "deadLetters",
                                      exchange: exchangeName, 
                                      routingKey: "");

                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (sender, eventArgs) =>
                    {
                        var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                        var deathReasonBytes = eventArgs
                                                    .BasicProperties
                                                    .Headers["x-first-death-reason"] as byte[];
                        var deathReason = Encoding.UTF8.GetString(deathReasonBytes);

                        Console.WriteLine($"DeadLetter: {message}. Rason: {deathReason}");

                    };

                    channel.BasicConsume(queue: "deadLetters",
                                         autoAck: true,
                                         consumer: consumer);
                    Console.WriteLine("Waiting...");
                    Console.ReadLine();
                }
            }
        }
    }
}
