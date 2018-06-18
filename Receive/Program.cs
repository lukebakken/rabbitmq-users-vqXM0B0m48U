using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;

class Receive
{
    private static readonly AutoResetEvent evt = new AutoResetEvent(false);

    public static void Main()
    {
        var factory = new ConnectionFactory() {
            HostName = "shostakovich",
            RequestedHeartbeat = 5,
            ContinuationTimeout = TimeSpan.FromSeconds(5),
            HandshakeContinuationTimeout = TimeSpan.FromSeconds(5),
            SocketReadTimeout = 2500,
            SocketWriteTimeout = 2500,
            AutomaticRecoveryEnabled = false,
            TopologyRecoveryEnabled = false
        };
        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };

                consumer.Shutdown += (model, e) =>
                {
                    Console.WriteLine(" Consumer shutdown, exiting!");
                    evt.Set();
                };

                consumer.ConsumerCancelled += (model, e) =>
                {
                    Console.WriteLine(" Consumer cancelled, exiting!");
                    evt.Set();
                };

                consumer.Registered += (model, e) =>
                {
                    Console.WriteLine(" Consumer registered");
                };

                consumer.Unregistered += (model, e) =>
                {
                    Console.WriteLine(" Consumer Un-registered");
                };

                channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

                Console.WriteLine(" Press CTRL-C to exit.");
                Console.CancelKeyPress += Console_CancelKeyPress;
                evt.WaitOne();
                Console.WriteLine(" EXITING!");
            }
        }
    }

    private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        evt.Set();
    }
}
