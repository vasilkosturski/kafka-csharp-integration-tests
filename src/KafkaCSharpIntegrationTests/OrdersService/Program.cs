namespace OrdersService;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        var app = builder.Build();
        app.MapControllers();
        await app.RunAsync();
    }
}