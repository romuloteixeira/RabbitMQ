using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

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
					var exchangeName = "myExchange";

					channel.ExchangeDeclare(exchange: exchangeName,
											type: ExchangeType.Direct);

					var message = GetMessage(args);
					var body = Encoding.UTF8.GetBytes(message);

					channel.BasicPublish(exchange: exchangeName,
										 routingKey: "",
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
			var newText = (args.Length > 1)
							? string.Join(" ", args.Skip(1).ToArray())
							: basicText;
			return newText;
		}
	}
}