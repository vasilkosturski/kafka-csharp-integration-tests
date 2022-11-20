using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace OrdersService;

public interface IKafkaProducer : IDisposable
{
    public Task Produce<TMessage>(string topic, TMessage message);
}

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<Null, string> kafkaProducer;
    
    private int disposed;

    public KafkaProducer(IOptions<KafkaOptions> kafkaOptions)
    {
        var bootstrapServers = kafkaOptions.Value.BootstrapServers;
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        
        kafkaProducer = new ProducerBuilder<Null, string>(config).Build();
    }
    
    public async Task Produce<TMessage>(string topic, TMessage message)
    {
        await kafkaProducer.ProduceAsync(topic, new Message<Null, string>
        {
            Value = JsonSerializer.Serialize(message)
        });
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1) return;
        kafkaProducer?.Flush();
        kafkaProducer?.Dispose();
    }
}