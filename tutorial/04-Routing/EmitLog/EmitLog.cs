using System;
using System.Text;
using System.Linq;
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
					var exchangeName = "direct_logs";
					
					channel.ExchangeDeclare(exchange: exchangeName,
											type: "direct");
					
					// var properties = channel.CreateBasicProperties();
					// properties.Persistent = true;
					
					var severity = (args.Length > 0) ? args[0] : "info";
					var message = GetMessage(args);
					var body = Encoding.UTF8.GetBytes(message);
					
					channel.BasicPublish(exchange: exchangeName,
										 routingKey: severity,
										 basicProperties: null,
										 body: body);
					
					Console.WriteLine($" [x] Sent {severity} {message}");
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