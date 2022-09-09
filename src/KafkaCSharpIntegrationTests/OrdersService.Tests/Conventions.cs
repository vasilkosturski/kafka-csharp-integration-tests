using AutoFixture;
using AutoFixture.Xunit2;

namespace OrdersService.Tests;

public class Conventions : AutoDataAttribute
{
    public Conventions() : base(() => new Fixture()
        .Customize(new TestServerCustomization()))
    {
        
    }
}

public class TestServerCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var client = new CustomWebApplicationFactory<Program>().CreateClient();
        fixture.Inject(client);
    }
}

public class Customization : ICustomization
{
    public void Customize(IFixture fixture)
    {
    }
}