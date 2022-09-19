using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;

namespace OrdersService.Tests;

public class Conventions : AutoDataAttribute
{
    public Conventions() : base(() => new Fixture()
        .Customize(new ConfigureTestContainers())
        .Customize(new ConfigureTopics())
        .Customize(new ConfigureTestServer())
        .Customize(new ConfigureKafkaConsumer()))
    {
    }
}

public class ContainersConfig
{
    public int KafkaHostPort { get; set; }
}

public class ConfigureTestContainers : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var zookeeperContainerName = $"zookeeper_{fixture.Create<string>()}"; 
        var zookeeperContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("confluentinc/cp-zookeeper:7.0.1")
            .WithName(zookeeperContainerName)
            .WithPortBinding(2181, true)
            .WithEnvironment(new Dictionary<string, string>
            {
                {"ZOOKEEPER_CLIENT_PORT", "2181"},
                {"ZOOKEEPER_TICK_TIME", "2000"}
            })
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(2181))
            .Build();

        AsyncContext.Run(async () => await zookeeperContainer.StartAsync());
        
        var zookeeperHostPort = zookeeperContainer.GetMappedPublicPort(2181);

        // UseAvailablePort

        var hostPort = GetAvailablePort();
        var kafkaContainerName = $"kafka_{fixture.Create<string>()}"; 
        var kafkaContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("confluentinc/cp-kafka:7.0.1")
            .WithName(kafkaContainerName)
            .WithHostname(kafkaContainerName)
            .WithPortBinding(hostPort, 9092)
            .WithEnvironment(new Dictionary<string, string>
            {
                {"KAFKA_BROKER_ID", "1"},
                {"KAFKA_ZOOKEEPER_CONNECT", $"host.docker.internal:{zookeeperHostPort}"},
                {"KAFKA_LISTENER_SECURITY_PROTOCOL_MAP", "PLAINTEXT:PLAINTEXT,PLAINTEXT_INTERNAL:PLAINTEXT"},
                {"KAFKA_LISTENERS", "PLAINTEXT://:9092,PLAINTEXT_INTERNAL://:29092"},
                {"KAFKA_ADVERTISED_LISTENERS", $"PLAINTEXT://localhost:{hostPort},PLAINTEXT_INTERNAL://{kafkaContainerName}:29092"},
                {"KAFKA_INTER_BROKER_LISTENER_NAME", "PLAINTEXT_INTERNAL"},
                {"KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1"},
                {"KAFKA_TRANSACTION_STATE_LOG_MIN_ISR", "1"},
                {"KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR", "1"},
            })
            .WithOutputConsumer(new OutputConsumer())
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(9092))
            .Build();

        AsyncContext.Run(async () => await kafkaContainer.StartAsync());
        
        fixture.Inject(new ContainersConfig
        {
            KafkaHostPort = hostPort
        });
    }
    
    private static readonly IPEndPoint defaultLoopbackEndpoint = new(IPAddress.Loopback, 0);
    public static int GetAvailablePort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(defaultLoopbackEndpoint);
        var port = ((IPEndPoint)socket.LocalEndPoint)!.Port;
        return port;
    }
}

public class OutputConsumer : IOutputConsumer
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public bool Enabled => true;

    public Stream Stdout => Console.OpenStandardOutput();
    public Stream Stderr => Console.OpenStandardOutput();
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
        var kafkaPort = fixture.Create<ContainersConfig>().KafkaHostPort;
        var bootstrapServers = $"localhost:{kafkaPort}";
        
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = Guid.NewGuid().ToString(),
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        
        // dispose?
        var consumer = new ConsumerBuilder<Null, Order>(config)
            .SetValueDeserializer(new CustomValueDeserializer<Order>())
            .Build();
        
        var topicNameMap = fixture.Create<Dictionary<Type, string>>();
        var topicName = topicNameMap[typeof(Order)];

        AsyncContext.Run(async () => await CreateKafkaTopic(topicName, bootstrapServers));

        consumer.Subscribe(topicName);
        
        fixture.Inject(consumer);
    }
    
    private static async Task CreateKafkaTopic(string topicName, string bootstrapServers)
    {
        using var adminClient =
            new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build();
        
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