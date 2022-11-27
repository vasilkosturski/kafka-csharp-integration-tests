using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace OrdersService;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IKafkaProducer producer;
    private readonly IOptions<KafkaOptions> kafkaOptions;

    public OrdersController(IKafkaProducer producer, IOptions<KafkaOptions> kafkaOptions)
    {
        this.producer = producer;
        this.kafkaOptions = kafkaOptions;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        await producer.Produce(kafkaOptions.Value.OrdersTopicName, order);
        return Ok();
    }
}