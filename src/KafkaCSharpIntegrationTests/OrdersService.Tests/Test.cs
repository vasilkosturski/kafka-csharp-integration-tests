using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace OrdersService.Tests;

public class Test : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public Test(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }
    
    [Theory]
    [InlineData("/api/orders")]
    public async Task OrdersTest(string url)
    {
        // Arrange
        var client = factory.CreateClient();
        
        await CreateKafkaTopic();
        
        // Act
        var request = new Order
        {
            Id = Guid.NewGuid().ToString(),
            Price = 10,
            Product = Product.Shirt
        };
        var ser = JsonSerializer.Serialize(request);
        var response = await client.PostAsync(url, new StringContent(ser, Encoding.UTF8, "application/json"));
        
        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
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