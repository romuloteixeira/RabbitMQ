using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
//using System.Threading;

namespace ReceiveLogs
{
    class ReceiveLog
    {
        static void Main(string[] args)
        {
			var factory = new ConnectionFactory { HostName = "localhost" };
			
			using (var connection = factory.CreateConnection())
			{
				using (var channel = connection.CreateModel())
				{
					var exchangeName = "logs";
					channel.ExchangeDeclare(exchange: exchangeName,
											 type: ExchangeType.Fanout);
											 
					var queueName = channel.QueueDeclare().QueueName;
					
					channel.QueueBind(queue: queueName,
									  exchange: exchangeName,
									  routingKey: "");
					
					Console.WriteLine(" [*] Waiting for logs.");
					
					var consumer = new EventingBasicConsumer(channel);
					
					consumer.Received += (model, ea) =>
					{
						var body = ea.Body.ToArray();
						var message = Encoding.UTF8.GetString(body);
						
						Console.WriteLine($" [x] {message} - {DateTime.Now}");
						
						//int dots = message.Split('.').Length - 1;
						
						//Thread.Sleep(dots * 100);
						
						//Console.WriteLine(" [x] Done.");
					};
					
					channel.BasicConsume(queue: queueName,
										 autoAck: true,
										 consumer: consumer);
					
					Console.WriteLine(" Press [enter] to exit.");
					Console.ReadLine();
				}
			}
        }
    }
}