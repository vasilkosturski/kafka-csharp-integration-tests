using Confluent.Kafka;

namespace OrdersService;

public class Producer<T>
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