using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace PrimoSender
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connessione = factory.CreateConnection())
            {
                using (var channel = connessione.CreateModel())
                {
                    channel.ConfirmSelect();
                    channel.QueueDeclare(queue: "sales",
                        durable: true, exclusive: false, autoDelete: false,
                         arguments: null);

                    channel.QueueDeclare(queue: "website",
                        durable: true, exclusive: false, autoDelete: false,
                        arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, mes) =>
                    {
                        var receivedBody = mes.Body.ToArray();
                        var receivedMessage = Encoding.UTF8.GetString(receivedBody);
                        Console.WriteLine($"Ho ricevuto {receivedMessage} with correlation ID {mes.BasicProperties.CorrelationId}");

                    };
                    channel.BasicConsume(queue: "website", autoAck: true,
                        consumer: consumer);

                    var messaggio = "Benvenuti alla prima demo di rabbit";
                    var body = Encoding.UTF8.GetBytes(messaggio);

                    var properties = channel.CreateBasicProperties();
                    properties.ReplyTo = "website";
                    properties.CorrelationId = $"ordine {Guid.NewGuid().ToString().Substring(0, 3)}";


                    for (int i = 0; i < 1000; i++)
                    {
                        channel.BasicPublish(exchange: "", routingKey: "sales",
                            body: body, basicProperties: properties);
                        Console.WriteLine($"Ho inviato {messaggio}");
                    }

//                    channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));

                }
            }
            Console.WriteLine("Premi invio per uscire");
            Console.ReadLine();


        }
    }
}
