namespace OrdersService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

        builder.Services.Configure<KafkaOptions>(builder.Configuration);

        var app = builder.Build();
        
        app.MapControllers();
        
        await app.RunAsync();
    }
}