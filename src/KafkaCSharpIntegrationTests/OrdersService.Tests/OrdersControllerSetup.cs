using AutoFixture;
using AutoFixture.Xunit2;

namespace OrdersService.Tests;

public class OrdersControllerSetup : AutoDataAttribute
{
    public OrdersControllerSetup() : base(() => new Fixture()
        .Customize(new TestContainersSetup())
        .Customize(new KafkaConsumerSetup())
        .Customize(new TestServerSetup()))
    {
    }
}