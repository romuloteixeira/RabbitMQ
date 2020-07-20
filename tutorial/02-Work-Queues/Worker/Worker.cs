using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace Worker
{
    class Worker
    {
        static void Main(string[] args)
        {
			var factory = new ConnectionFactory { HostName = "localhost" };
			
			using (var connection = factory.CreateConnection())
			{
				using (var channel = connection.CreateModel())
				{
					channel.QueueDeclare(queue: "task_queue",
										 durable: true,
										 exclusive: false,
										 autoDelete: false,
										 arguments: null);
					
					
					// Send only after the consumer/worker finalliese it.
					channel.BasicQos(prefetchSize: 0, 
									 prefetchCount: 1, 
									 global: false);
					
					Console.WriteLine(" [*] Waiting for messages.");
					
					var consumer = new EventingBasicConsumer(channel);
					
					consumer.Received += (sender, ea) =>
					{
						var body = ea.Body.ToArray();
						var message = Encoding.UTF8.GetString(body);
						
						Console.WriteLine($" [x] Received {message} - {DateTime.Now}");
						
						int dots = message.Split('.').Length - 1;
						
						Thread.Sleep(dots * 100);
						
						Console.WriteLine(" [x] Done.");
						
						// Note: It is possible to access the channel via
						//       ((EnentingBasicConsumer)sender).Model here
						channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
					};
					
					channel.BasicConsume(queue: "task_queue",
										 autoAck: false,
										 consumer: consumer);
					
					Console.WriteLine(" Press [enter] to exit.");
					Console.ReadLine();
				}
			}
        }
    }
}