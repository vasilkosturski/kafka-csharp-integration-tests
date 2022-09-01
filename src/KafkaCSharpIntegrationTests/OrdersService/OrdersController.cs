using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IProducer<Null, Order> producer;
    
    public OrdersController(OrdersProducerFactory producerFactory)
    {
        producer = producerFactory.Get();
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Order order)
    {
        await producer.ProduceAsync("orders", new Message<Null, Order> { Value = order });
        return await Task.FromResult(Ok());
    }
}