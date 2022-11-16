using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace OrdersService;

public class ProducerFactory
{
    private readonly object lockHandle = new ();
    
    private readonly Dictionary<Type, string> topicNameMap;
    private readonly IOptions<KafkaOptions> kafkaOptions;
    private readonly Dictionary<Type, object> producers = new();

    public ProducerFactory(Dictionary<Type, string> topicNameMap, IOptions<KafkaOptions> kafkaOptions)
    {
        this.topicNameMap = topicNameMap;
        this.kafkaOptions = kafkaOptions;
    }
    
    public IProducer<T> Get<T>()
    {
        lock (lockHandle)
        {
            if (!producers.ContainsKey(typeof(T)))
            {
                var topicName = topicNameMap[typeof(T)];
                var bootstrapServers = kafkaOptions.Value.BootstrapServers;
                var config = new ProducerConfig { BootstrapServers = bootstrapServers };
                var kafkaProducer = new ProducerBuilder<Null, T>(config)
                    .SetValueSerializer(new JsonSerializer<T>())
                    .Build();
                var producer = new Producer<T>(kafkaProducer, topicName);
                producers.Add(typeof(T), producer);
            }

            return (IProducer<T>)producers[typeof(T)];
        }
    }
}

public interface IProducer<in T>
{
    Task ProduceAsync(T message);
}

public class Producer<T> : IProducer<T>
{
    private readonly IProducer<Null, T> kafkaProducer;
    private readonly string topicName;

    public Producer(IProducer<Null, T> kafkaProducer, string topicName)
    {
        this.kafkaProducer = kafkaProducer;
        this.topicName = topicName;
    }
    
    public async Task ProduceAsync(T message)
    {
        await kafkaProducer.ProduceAsync(topicName, new Message<Null, T> { Value = message });
    }
}

