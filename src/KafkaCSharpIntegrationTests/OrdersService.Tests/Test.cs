using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using FluentAssertions;
using Xunit;

namespace OrdersService.Tests;

public class Test
{
    [Theory]
    [Conventions]
    public async Task OrdersTest(HttpClient client)
    {
        // Arrange
        //var client = factory.CreateClient();
        
        await CreateKafkaTopic();
        
        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            Price = 10,
            Product = Product.Shirt
        };
        var ser = JsonSerializer.Serialize(order);
        
        // Act
        
        var response = await client.PostAsync("/api/orders", new StringContent(ser, Encoding.UTF8, "application/json"));
        
        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = Guid.NewGuid().ToString(),
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using (var consumer = new ConsumerBuilder<Null, Order>(config)
                   .SetValueDeserializer(new CustomValueDeserializer<Order>())
                   .Build())
        {
            consumer.Subscribe("orders");

            var res = consumer.Consume(TimeSpan.FromSeconds(10));

            res.Message.Value.Id.Should().Be(order.Id);
            res.Message.Value.Price.Should().Be(order.Price);
            res.Message.Value.Product.Should().Be(order.Product);
        }
    }

    private static async Task CreateKafkaTopic()
    {
        using var adminClient =
            new AdminClientBuilder(new AdminClientConfig { BootstrapServers = "localhost:9092" }).Build();
        
        try
        {
            await adminClient.CreateTopicsAsync(new TopicSpecification[]
            {
                new() { Name = "orders", ReplicationFactor = 1, NumPartitions = 1 }
            });
        }
        catch (CreateTopicsException e)
        {
            Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
        }
    }
}