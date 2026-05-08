using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orders.API.Configuration;
using Orders.API.Services;
using Serilog;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("🚀 Iniciando Orders API");

    var builder = WebApplication.CreateBuilder(args);

    // Usar Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers();

    // Configurar RabbitMQ Settings
    builder.Services.Configure<RabbitMQSettings>(
        builder.Configuration.GetSection("RabbitMQ"));

    // Registrar RabbitMQ Producer como Singleton
    builder.Services.AddSingleton<IRabbitMQProducer>(sp =>
    {
        var producer = new RabbitMQProducer(
            sp.GetRequiredService<ILogger<RabbitMQProducer>>(),
            sp.GetRequiredService<IOptions<RabbitMQSettings>>());

        // Conectar al inicio
        producer.Connect();

        return producer;
    });

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseCors("AllowAll");
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("✅ Orders API iniciada correctamente");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Error fatal al iniciar la aplicación");
}
finally
{
    Log.CloseAndFlush();
}
