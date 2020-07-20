using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EmitLog
{
    class EmitLog
    {
		static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = factory.CreateConnection();
            using (var channel = connection.CreateModel())
            {
                var queueName = "task_queue";
                //var exchangeName = string.Empty;
                //channel.ExchangeDeclare(exchange: exchangeName,
                //                     type: ExchangeType.Fanout);

                const string ExchangeName = "DeadLetterExchange";
                var arguments = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", ExchangeName }
                };
                channel.QueueDeclare(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: arguments);

                var message = string.Empty;
                message = GetMessage(args);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: string.Empty,
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);

                Console.WriteLine($" [x] Sent {message}");
            }
        }

		private static string GetMessage(string[] args)
		{
			var now = DateTime.Now;
			var basicText = $"Hello Word! At {now}";
			var personalizeText = $"{string.Join(" ", args)} {now}";
                
            return ((args.Length > 0) ? personalizeText : basicText);
		}
	}
}
