using System;
using System.Collections.Generic;
using AutoFixture;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace OrdersService.Tests;

public class CustomWebApplicationFactory<TStartup>
    : WebApplicationFactory<TStartup> where TStartup: class
{
    private readonly Fixture fixture = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(new Dictionary<Type, string>
            {
                { typeof(Order), $"orders-{fixture.Create<string>()}" }
            });
        });
    }
}