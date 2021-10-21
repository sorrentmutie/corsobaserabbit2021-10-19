using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace EcommerceWeb
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory
               { HostName = "localhost", VirtualHost = "ecommerce",
             UserName="mario", Password = "password"};

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ConfirmSelect();

                    channel.QueueDeclare(
                        queue: "website", durable: true,
                        exclusive: false, autoDelete: false,
                        arguments: null);

                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (model, ea) =>
                    {
                        var receiverBody = ea.Body.ToArray();
                        var receivedMessage = Encoding.UTF8.GetString((receiverBody));
                        Console.WriteLine($"Received {receivedMessage} with correlation Id {ea.BasicProperties.CorrelationId}");
                    };

                    channel.BasicConsume(queue: "website", autoAck: true, consumer: consumer);

                    string message = "Sta arrivando un ordine";
                    var body = Encoding.UTF8.GetBytes(message);
                    var props = channel.CreateBasicProperties();
                    props.ReplyTo = "website";
                    props.CorrelationId = Guid.NewGuid().ToString();


                    channel.BasicPublish(exchange: "", routingKey: "sales",
                        body: body, basicProperties: props);

                }
            }

            Console.ReadLine();
        }
    }
}
