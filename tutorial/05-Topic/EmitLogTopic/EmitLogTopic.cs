using System;
using System.Text;
using System.Linq;
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
					var exchangeName = "topic_logs";
					channel.ExchangeDeclare(exchange: exchangeName,
											type: "topic");
					
					var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";
					var message = GetMessage(args);
					var body = Encoding.UTF8.GetBytes(message);
					
					channel.BasicPublish(exchange: exchangeName,
										 routingKey: routingKey,
										 basicProperties: null,
										 body: body);
					
					Console.WriteLine($" [x] Sent {routingKey} {message}");
				}
			}
        }
		
		private static string GetMessage(string[] args)
		{
			var now = DateTime.Now;
			var basicText = $"Hello Word! At {now}";
			var newText = (args.Length > 1)
							? string.Join(" ", args.Skip( 1 ).ToArray())
							: basicText;
			return newText;
		}
    }
}