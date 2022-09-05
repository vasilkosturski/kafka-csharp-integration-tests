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
        using var ms = new MemoryStream();
        
        var jsonString = JsonSerializer.Serialize(data);
        var writer = new StreamWriter(ms);

        writer.Write(jsonString);
        writer.Flush();
        ms.Position = 0;

        return ms.ToArray();
    }
}

public class CustomValueDeserializer<T> : IDeserializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return JsonSerializer.Deserialize<T>(data.ToArray());
    }
}

