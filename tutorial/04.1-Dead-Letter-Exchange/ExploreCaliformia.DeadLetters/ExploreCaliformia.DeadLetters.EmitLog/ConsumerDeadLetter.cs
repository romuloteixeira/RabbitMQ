using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExploreCaliformia.DeadLetters.ConsumerLogs2
{
	class ConsumerDeadLetter
    {
		static void Main(string[] args)
		{
			var factory = new ConnectionFactory { HostName = "localhost" };

			using (var connection = factory.CreateConnection())
			{
				using (var channel = connection.CreateModel())
				{
					const string exchangeNameDeadLetter = "DLX";
					const string exchangeName = "myExchange";
					var arguments = new Dictionary<string, object>
					{
						{ "x-dead-letter-exchange", exchangeNameDeadLetter },
					};

					channel.ExchangeDeclare(exchange: exchangeName,
											type: ExchangeType.Direct);

					const string queueName = "backOfficeQueue";
                    channel.QueueDeclare(queue: queueName,
										 durable: true,
										 exclusive: false,
										 autoDelete: false,
										 arguments: arguments);
					var headers = new Dictionary<string, object>
					{
						{ "subject", "toour" },
						{ "action", "booked" },
						{ "x-match", "any" },
					};

					channel.QueueBind(queue: queueName,
									  exchange: exchangeName,
									  routingKey: "",
									  arguments: headers);
					var consumer = new EventingBasicConsumer(channel);
					var isReject = true;
					consumer.Received += (sender, eventArgs) =>
					{
                        try
						{
							var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
							var userId = eventArgs.BasicProperties.UserId;

							if (isReject)
							{
								channel.BasicReject(eventArgs.DeliveryTag, false);

							}
							else
							{
								channel.BasicAck(eventArgs.DeliveryTag, false);
							}
							Console.WriteLine($"{userId} -> {message} - was reject: {isReject}");

							isReject = !isReject;
						}
						catch
                        {

                        }
					};

					channel.BasicConsume(queue: queueName,
										 autoAck: false,
										 consumer: consumer);

                    Console.WriteLine("Waiting...");
					Console.ReadLine();
				}
			}
		}
	}
}
