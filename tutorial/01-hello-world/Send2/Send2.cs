using System;
using System.Text;
using RabbitMQ.Client;
using System.Threading;

namespace Send2
{
    class Send2
    {
        static void Main()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
			var random = new Random();
			
			for(var i = 0; i < 100; i++)
			{
				using (var connection = factory.CreateConnection())
				{
					using (var channel = connection.CreateModel())
					{
						channel.QueueDeclare(queue: "hello",
											 durable: false,
											 exclusive: false,
											 autoDelete: false,
											 arguments: null);
						
						var message = $"Send 2 - Hello World! Teste {DateTime.Now}";
						var body = Encoding.UTF8.GetBytes(message);
						
						channel.BasicPublish(exchange: "",
											 routingKey: "hello",
											 basicProperties: null,
											 body: body);
						
						Console.WriteLine($" [x] Sent {message}");
					}
				}
				
				Thread.Sleep(random.Next(50, 150));
			}
			
			Console.WriteLine(" Press [enter] to exit.");
			Console.ReadLine();
        }
    }
}
