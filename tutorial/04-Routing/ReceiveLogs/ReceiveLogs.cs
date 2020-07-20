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
					var exchangeName = "direct_logs";
					channel.ExchangeDeclare(exchange: exchangeName,
											 type: "direct");
											 
					var queueName = channel.QueueDeclare().QueueName;
					
					if (args.Length < 1)
					{
						Console.Error.WriteLine($"Usage {Environment.GetCommandLineArgs()[0]} "
												+ "[info] [warning] [error]");
						Console.WriteLine(" Press [enter] to exit");
						Console.ReadLine();
						Environment.ExitCode = 1;
						return;
					}
					
					foreach (var severity in args)
					{
						channel.QueueBind(queue: queueName,
										  exchange: exchangeName,
										  routingKey: severity);
					}
					
					
					Console.WriteLine(" [*] Waiting for logs.");
					
					var consumer = new EventingBasicConsumer(channel);
					
					consumer.Received += (model, ea) =>
					{
						var body = ea.Body.ToArray();
						var message = Encoding.UTF8.GetString(body);
						var routingKey = ea.RoutingKey;
						
						Console.WriteLine($" [x] Received {routingKey} {message} "
										  + $"- {DateTime.Now}");
						
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