using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Nito.AsyncEx;

namespace OrdersService.Tests;

public class Conventions : AutoDataAttribute
{
    public Conventions() : base(() => new Fixture()
        .Customize(new ConfigureTopics())
        .Customize(new ConfigureTestServer())
        .Customize(new ConfigureKafkaConsumer()))
    {
    }
}

public class ConfigureTopics : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Inject(new Dictionary<Type, string>
        {
            { typeof(Order), $"orders-{fixture.Create<string>()}" }
        });
    }
}

public class ConfigureTestServer : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var client = new CustomWebApplicationFactory<Program>(fixture).CreateClient();
        fixture.Inject(client);
    }
}

public class ConfigureKafkaConsumer : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = Guid.NewGuid().ToString(),
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        
        // dispose?
        var consumer = new ConsumerBuilder<Null, Order>(config)
            .SetValueDeserializer(new CustomValueDeserializer<Order>())
            .Build();
        
        var topicNameMap = fixture.Create<Dictionary<Type, string>>();
        var topicName = topicNameMap[typeof(Order)];

        AsyncContext.Run(async () => await CreateKafkaTopic(topicName));

        consumer.Subscribe(topicName);
        
        fixture.Inject(consumer);
    }
    
    private static async Task CreateKafkaTopic(string topicName)
    {
        using var adminClient =
            new AdminClientBuilder(new AdminClientConfig { BootstrapServers = "localhost:9092" }).Build();
        
        try
        {
            await adminClient.CreateTopicsAsync(new TopicSpecification[]
            {
                new() { Name = topicName, ReplicationFactor = 1, NumPartitions = 1 }
            });
        }
        catch (CreateTopicsException e)
        {
            Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
        }
    }
}