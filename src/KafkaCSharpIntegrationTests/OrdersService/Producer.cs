using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace OrdersService;

public interface IKafkaProducer : IDisposable
{
    public Task Produce<TMessage>(TMessage message);
}

public class KafkaProducer : IKafkaProducer
{
    private readonly Dictionary<Type, string> topicNameMap;
    private readonly IProducer<Null, string> kafkaProducer;
    
    private int disposed;

    public KafkaProducer(Dictionary<Type, string> topicNameMap, IOptions<KafkaOptions> kafkaOptions)
    {
        this.topicNameMap = topicNameMap;
        var bootstrapServers = kafkaOptions.Value.BootstrapServers;
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        
        kafkaProducer = new ProducerBuilder<Null, string>(config)
            .Build();
    }
    
    public async Task Produce<TMessage>(TMessage message)
    {
        var topic = topicNameMap[typeof(TMessage)];
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