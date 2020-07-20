using System;
using System.Text;
//using System.Threading;
using RabbitMQ.Client;

namespace EmitLogs
{
    class EmitLog
    {
        static void Main(string[] args)
        {
			var factory = new ConnectionFactory { HostName = "localhost" };
			
			using (var connection = factory.CreateConnection())
			{
				using (var channel = connection.CreateModel())
				{
					var queueName = string.Empty;
					var exchangeName = "logs";
					
					channel.ExchangeDeclare(exchange: exchangeName,
										 type: ExchangeType.Fanout);
					
					// var properties = channel.CreateBasicProperties();
					// properties.Persistent = true;
					
					var message = string.Empty;
					message = GetMessage(args);
					var body = Encoding.UTF8.GetBytes(message);
					
					channel.BasicPublish(exchange: exchangeName,
									 routingKey: queueName,
									 basicProperties: null,
									 body: body);
					
					Console.WriteLine($" [x] Sent {message}");
				}
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