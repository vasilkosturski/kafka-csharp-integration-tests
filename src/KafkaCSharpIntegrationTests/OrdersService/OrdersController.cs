using Microsoft.AspNetCore.Mvc;

namespace OrdersService;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Order order)
    {
        return await Task.FromResult(Ok());
    }
}