using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

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
					var exchangeName = "topic_logs";
					channel.ExchangeDeclare(exchange: exchangeName,
											 type: "topic");
											 
					var queueName = channel
										.QueueDeclare(autoDelete: false, durable: true)
										.QueueName;
					
					if (args.Length < 1)
					{
						Console.Error.WriteLine($"Usage: {Environment.GetCommandLineArgs()[0]} "
												+ "[binding_key]");
												
						Console.WriteLine(" Press [enter] to exit");
						Console.ReadLine();
						Environment.ExitCode = 1;
						return;
					}
					
					var currentBindinKey = string.Empty;
					foreach (var bindingKey in args)
					{
						channel.QueueBind(queue: queueName,
										  exchange: exchangeName,
										  routingKey: bindingKey);
						
						currentBindinKey += bindingKey + " ";
					}
					
					var consumer = new EventingBasicConsumer(channel);
					
					consumer.Received += (sender, eventArgs) =>
					{
						var body = eventArgs.Body.ToArray();
						var message = Encoding.UTF8.GetString(body);
						var routingKey = eventArgs.RoutingKey;
						
						Console.WriteLine($" [x] Received {routingKey} {message} "
										  + $"- {DateTime.Now}");
						
						Thread.Sleep(500);
						
						Console.WriteLine(" [x] Done.");
						Console.WriteLine($" Wainting for {currentBindinKey}");
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