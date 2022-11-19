using AutoFixture;

namespace OrdersService.Tests;

public class TestServerSetup : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var client = new CustomWebApplicationFactory<Program>(fixture).CreateClient();
        fixture.Inject(client);
    }
}