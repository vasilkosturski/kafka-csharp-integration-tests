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
    public async Task PushOrderToKafka(HttpClient client, IConsumer<Null, string> consumer, Order order)
    {
        // Act
        var response = await client.PostAsync("/api/orders", 
            new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json"));
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        var consumeResult = consumer.Consume(TimeSpan.FromSeconds(10));
        var consumedOrder = JsonSerializer.Deserialize<Order>(consumeResult.Message.Value);
        
        consumedOrder.Id.Should().Be(order.Id);
        consumedOrder.Price.Should().Be(order.Price);
        consumedOrder.Product.Should().Be(order.Product);
    }
}