using Microsoft.AspNetCore.Mvc;

namespace OrdersService;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<string>> Get()
    {
        return await Task.FromResult(Ok("hui"));
    }
}