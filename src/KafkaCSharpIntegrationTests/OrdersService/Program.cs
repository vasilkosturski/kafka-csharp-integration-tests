namespace OrdersService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        builder.Services.AddSingleton<ProducerFactory>();

        builder.Services.AddSingleton(new Dictionary<Type, string>
        {
            { typeof(Order), "orders" }
        });

        var app = builder.Build();
        app.MapControllers();
        await app.RunAsync();
    }
}