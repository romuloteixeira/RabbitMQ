using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;

namespace NewTask
{
    class NewTask
    {
        static void Main(string[] args)
        {
			var factory = new ConnectionFactory { HostName = "localhost" };
			//var random = new Random();
			
			using (var connection = factory.CreateConnection())
			{
				using (var channel = connection.CreateModel())
				{
					channel.QueueDeclare(queue: "task_queue",
										 durable: true,
										 exclusive: false,
										 autoDelete: false,
										 arguments: null);
					
					var properties = channel.CreateBasicProperties();
					properties.Persistent = true;
					
					var message = string.Empty;
					
					for (var i = 1; i < 5000; i++)
					{
						message = GetMessage(args);
						var body = Encoding.UTF8.GetBytes(message);
						
						channel.BasicPublish(exchange: "",
										 routingKey: "task_queue",
										 basicProperties: properties,
										 body: body);
										 
						//Thread.Sleep(50);
					}
					
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