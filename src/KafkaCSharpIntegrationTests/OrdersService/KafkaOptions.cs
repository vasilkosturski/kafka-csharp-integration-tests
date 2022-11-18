namespace OrdersService;

public class KafkaOptions
{
    public string BootstrapServers { get; set; }
    public string OrdersTopicName { get; set; }
}