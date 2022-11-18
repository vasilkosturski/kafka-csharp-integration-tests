using Microsoft.AspNetCore.Mvc;

namespace OrdersService;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IKafkaProducer producer;

    public OrdersController(IKafkaProducer producer) =>
        this.producer = producer;

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Order order)
    {
        await producer.Produce(order);
        return Ok();
    }
}