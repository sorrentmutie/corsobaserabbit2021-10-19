using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace PrimoReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ConfirmSelect();
                    channel.QueueDeclare(queue: "sales",
                        durable: true, exclusive: false, autoDelete: false,
                        arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                       
                        var message = Encoding.UTF8.GetString(body);
                        var properties = ea.BasicProperties;

                        var replyProperties = channel.CreateBasicProperties();
                        replyProperties.CorrelationId = ea.BasicProperties.CorrelationId;

                        Console.WriteLine($"Ho ricevuto {message}");
                        var numeroRandom = new Random().Next(1, 4);
                        System.Threading.Thread.Sleep(numeroRandom * 1000);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                        //var replyMessage = $"order {ea.BasicProperties.CorrelationId} in progress..";
                        //var replyBody = Encoding.UTF8.GetBytes(replyMessage);
                        //channel.BasicPublish(exchange: "", routingKey: ea.BasicProperties.ReplyTo,
                        //     basicProperties: replyProperties, body: replyBody);
                        //channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                        //Console.WriteLine($"Ho inviato {replyMessage}");

                        //System.Threading.Thread.Sleep(2000);

                        //var shippedProperties = channel.CreateBasicProperties();
                        //shippedProperties.CorrelationId = ea.BasicProperties.CorrelationId;
                        //var shippedMessage = $"order {ea.BasicProperties.CorrelationId} shipped";
                        //var shippedBody = Encoding.UTF8.GetBytes(shippedMessage);
                        //channel.BasicPublish(exchange: "",
                        //    routingKey: ea.BasicProperties.ReplyTo,
                        //    body: shippedBody);
                        //channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                        //Console.WriteLine($" Spedito {shippedMessage}");

                    };
                    channel.BasicConsume(queue: "sales",
                        consumer: consumer, autoAck: false);

                    Console.WriteLine("Premi [enter] per uscire");
                    Console.ReadLine();
                }
            }
        }

        
    }
}
