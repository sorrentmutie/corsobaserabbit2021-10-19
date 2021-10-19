using RabbitMQ.Client;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EmitLog
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //var bindingKey = "critical";
            //if(args.Length >=1)
            //{
            //    foreach (var argument in args)
            //    {
            //        bindingKey += argument; 
            //    }
            //}
            //Console.WriteLine(bindingKey);
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

                    //  channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);
                    // channel.ExchangeDeclare(exchange: "direct_logs", type: ExchangeType.Direct);
                    channel.ExchangeDeclare(exchange: "topic_logs", type: ExchangeType.Topic);
                    var random = new Random();
                    for (int i = 0; i < 10000; i++)
                    {
                        var body = CreateMessage(random);
                        var severity = CreateSeverity(random);
                        Console.WriteLine(i);
                        //channel.BasicPublish(exchange: "logs",
                        //                        routingKey: "", basicProperties: null, body: body);
                        channel.BasicPublish(exchange: "topic_logs",
                        routingKey: "server.kern." + severity , basicProperties: null, body: body);

                    }
                }
            }
            Console.WriteLine("Press [enter] to exit");
            Console.ReadLine();
        }

        static async Task<string> ReadFile(string path)
        {
            using var reader = new StreamReader(path);
            var contenuto = await reader.ReadToEndAsync();
            return contenuto;
        }

        static string CreateSeverity(Random random)
        {
            var numeroRandom = random.Next(1, 100);
            if(numeroRandom <= 50)
            {
                return "info";
            } else if(numeroRandom > 50 && numeroRandom <75)
            {
                return "warning";
            } else
            {
                return "critical";
            }
        }

        static byte[] CreateMessage(Random random)
        {
            var numeroRandom = random.Next(1, 1000);
            Console.WriteLine($"Sto per spedire il messaggio {numeroRandom}");
            var body = Encoding.UTF8.GetBytes(numeroRandom.ToString());
            return body;
        }

    }
}
