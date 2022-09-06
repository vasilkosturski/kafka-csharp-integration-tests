using Microsoft.AspNetCore.Mvc;

namespace OrdersService;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ProducerFactory producerFactory;

    public OrdersController(ProducerFactory producerFactory)
    {
        this.producerFactory = producerFactory;
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Order order)
    {
        var producer = producerFactory.Get<Order>();
        await producer.ProduceAsync(order);
        return await Task.FromResult(Ok());
    }
}