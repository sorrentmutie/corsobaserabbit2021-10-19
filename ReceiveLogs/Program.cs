using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReceiveLogs
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost"};
            var severityList = new List<string>();
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    if(args.Length > 1)
                    {
                        foreach (var argument in args)
                        {
                            severityList.Add(argument);
                        }
                    } else
                    {
                        severityList.Add("*.critical");
                    }


                    //channel.ExchangeDeclare(exchange: "logs",
                    //    type: ExchangeType.Fanout);
                    //channel.ExchangeDeclare(exchange: "direct_logs",
                    //   type: ExchangeType.Direct);
                    channel.ExchangeDeclare(exchange: "topic_logs",
                       type: ExchangeType.Topic);

                    var queueName = channel.QueueDeclare().QueueName;
                    // sono interessato solo ai log con severity critical

                    //channel.QueueBind(queue: queueName, exchange: "logs",
                    //    routingKey: "");
                    //channel.QueueBind(queue: queueName, exchange: "direct_logs",
                    //    routingKey: "critical");

                    //channel.QueueBind(queue: queueName, exchange: "direct_logs",
                    //    routingKey: "warning");
                    //foreach (var severity in severityList)
                    //{
                    //    channel.QueueBind(queue: queueName, exchange: "topics_logs",
                    //        routingKey: severity);
                    //}

                    channel.QueueBind(queue: queueName, exchange: "topic_logs",
                            routingKey: "server.*.critical");

                    Console.WriteLine("Sono in attesa dei log");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, message) =>
                    {
                        var body = message.Body.ToArray();
                        var textMessage = Encoding.UTF8.GetString(body);
                        var routingKey = message.RoutingKey;
                        Console.WriteLine($"Messaggio: {textMessage} {routingKey}");
                    };
                    channel.BasicConsume(queue: queueName,
                        autoAck: true, consumer: consumer);
                    Console.WriteLine("Premi enter per uscire");
                    Console.ReadLine();
                }
            }
        }
    }
}
