using notification_worker.Configuration;
using notification_worker.Services;
using Serilog;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("🚀 Iniciando Notification Worker");

    var builder = Host.CreateApplicationBuilder(args);

    // Usar Serilog
    builder.Services.AddSerilog();

    // Configurar RabbitMQ Settings
    builder.Services.Configure<RabbitMQSettings>(
        builder.Configuration.GetSection("RabbitMQ"));

    // Registrar el servicio consumidor
    builder.Services.AddHostedService<OrderConsumerService>();

    var host = builder.Build();

    Log.Information("✅ Notification Worker iniciado correctamente");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Error fatal al iniciar el worker");
}
finally
{
    Log.CloseAndFlush();
}
