using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace OrdersService;

internal class Program
{
    public static async Task Main(string[] args)
    {
        //await TestKafka();
        
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        builder.Services.AddSingleton<OrdersProducerFactory>();

        var app = builder.Build();
        app.MapControllers();
        await app.RunAsync();
    }

    private static async Task TestKafka()
    {
        using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = "localhost:9092" }).Build())
        {
            try
            {
                await adminClient.CreateTopicsAsync(new TopicSpecification[] { 
                    new() { Name = "myTopic", ReplicationFactor = 1, NumPartitions = 1 } });
            }
            catch (CreateTopicsException e)
            {
                Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
            }
        }
        
        var config = new ProducerConfig { BootstrapServers = "localhost:9092" };
        
        // If serializers are not specified, default serializers from
        // `Confluent.Kafka.Serializers` will be automatically used where
        // available. Note: by default strings are encoded as UTF8.
        using (var p = new ProducerBuilder<Null, string>(config).Build())
        {
            try
            {
                var dr = await p.ProduceAsync("myTopic", new Message<Null, string> { Value="test" });
                Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }
        }
        
        var cConfig= new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            //SaslMechanism = SaslMechanism.Plain,
            //SecurityProtocol = SecurityProtocol.SaslSsl,
            //SaslUsername = "xxxxxxx",
            //SaslPassword = "xxxxx+",
            GroupId = Guid.NewGuid().ToString(),
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        
        using (var c = new ConsumerBuilder<Ignore, string>(cConfig).Build())
        {
            c.Subscribe("myTopic");
 
            var cts = new CancellationTokenSource();

            try
            {
                while (true)
                {
                    try
                    {
                        var cr = c.Consume(cts.Token);
                        Console.WriteLine($"Consumed message '{cr.Value}' at: '{cr.TopicPartitionOffset}'.");
                        await Task.Delay(1000, cts.Token);
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error occured: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Close and Release all the resources held by this consumer  
                c.Close();
            }
        }
    }
}