using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading;

namespace Send
{
    class Send
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
			var random = new Random();
			
			using (var connection = factory.CreateConnection())
			{
				using (var channel = connection.CreateModel())
				{
					for(var i = 0; i < 100; i++)
					{
						channel.QueueDeclare(queue: "hello",
											 durable: false,
											 exclusive: false,
											 autoDelete: false,
											 arguments: null);
						 
						var message = $"Send 1 - Hello World! Teste {DateTime.Now}";
						var body = Encoding.UTF8.GetBytes(message);
						
						channel.BasicPublish(exchange: "",
											 routingKey: "hello",
											 basicProperties: null,
											 body: body);
						
						Console.WriteLine($" [x] Sent {message}");
						
						Thread.Sleep(random.Next(10, 50));
					}
				}
			}
			
			Console.WriteLine(" Press [enter] to exit.");
			Console.ReadLine();
        }
    }
}
