using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace OrdersService;

public class OrdersProducerFactory : IDisposable
{
    private readonly IProducer<Null, Order> producer;
    
    public OrdersProducerFactory()
    {
        var config = new ProducerConfig { BootstrapServers = "localhost:9092" };
        producer = new ProducerBuilder<Null, Order>(config)
            .SetValueSerializer(new CustomValueSerializer<Order>())
            .Build();
    }

    public IProducer<Null, Order> Get() => producer; 

    public void Dispose()
    {
        producer?.Dispose();
    }
}

public class CustomValueSerializer<T> : ISerializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, typeof(T)));
    }
}

