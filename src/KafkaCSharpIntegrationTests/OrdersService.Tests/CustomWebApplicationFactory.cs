using AutoFixture;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace OrdersService.Tests;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup: class
{
    private readonly IFixture fixture;

    public CustomWebApplicationFactory(IFixture fixture) => this.fixture = fixture;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var kafkaTestConfig = fixture.Create<KafkaTestConfig>();
            services.Configure<KafkaOptions>(opts =>
            {
                opts.OrdersTopicName = kafkaTestConfig.TopicName;
                opts.BootstrapServers = kafkaTestConfig.BootstrapServers;
            });
        });
    }
}