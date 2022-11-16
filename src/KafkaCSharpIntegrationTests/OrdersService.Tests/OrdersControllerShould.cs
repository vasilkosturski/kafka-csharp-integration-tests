using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using FluentAssertions;
using Xunit;

namespace OrdersService.Tests;

public class OrdersControllerShould
{
    [Theory]
    [TestSetup]
    public async Task PushOrderToKafka(HttpClient client, IConsumer<Null, Order> consumer, Order order)
    {
        // Act
        var response = await client.PostAsync("/api/orders", 
            new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json"));
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        var res = consumer.Consume(TimeSpan.FromSeconds(10));

        res.Message.Value.Id.Should().Be(order.Id);
        res.Message.Value.Price.Should().Be(order.Price);
        res.Message.Value.Product.Should().Be(order.Product);
    }
}