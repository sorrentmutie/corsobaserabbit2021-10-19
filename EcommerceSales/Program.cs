using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace EcommerceSales
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                VirtualHost = "ecommerce",
                UserName = "mario",
                Password = "password"
            };

            Console.WriteLine("In attesa..");

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ConfirmSelect();
                    channel.QueueDeclare(
                      queue: "sales", durable: true,
                      exclusive: false, autoDelete: false,
                      arguments: null);
                    channel.ExchangeDeclare(exchange: "order.accepted",
                        durable: true, type: "topic");
                    channel.ExchangeDeclare(exchange: "order.rejected",
                        durable: true, type: "topic");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var receivedBody = ea.Body.ToArray();
                        var receivedMessage = Encoding.UTF8.GetString(receivedBody);
                        var receivedProps = ea.BasicProperties;

                        Console.WriteLine($"MESSAGGIO DA WEBSITE: {receivedMessage} {receivedProps.CorrelationId}");
                        // logica di procressamento ordine
                        System.Threading.Thread.Sleep(2000);

                        string replyMessage = $"Ordine {receivedProps.CorrelationId} è stato processato";
                        var replyBody = Encoding.UTF8.GetBytes(replyMessage);

                        channel.BasicPublish(exchange: "",
                            routingKey: "website", body: replyBody);

                        string eventMessage = $"Ordine {receivedProps.CorrelationId} è stato accettato";
                        var eventBody = Encoding.UTF8.GetBytes(eventMessage);
                        var eventProps = channel.CreateBasicProperties();
                        eventProps.CorrelationId = receivedProps.CorrelationId;

                        channel.BasicPublish(exchange: "order.accepted",
                                routingKey: "order.accepted", body: eventBody,
                                basicProperties: eventProps);

                        Console.WriteLine($"Ho pubblicato {eventMessage}");

                    };
                    channel.BasicConsume(queue: "sales", autoAck: true, consumer: consumer);


                }
            }

            Console.ReadLine();
        }
    }
}
