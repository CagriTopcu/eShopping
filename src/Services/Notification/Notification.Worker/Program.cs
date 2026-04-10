using MassTransit;
using Notification.Worker.Consumers;
using Notification.Worker.Services;
using ServiceDefaults;
using ServiceDefaults.CorrelationId;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<IEmailSender, ConsoleEmailSender>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConfirmedNotificationConsumer>();
    x.AddConsumer<OrderCancelledNotificationConsumer>();
    x.AddConsumer<PaymentFailedNotificationConsumer>();
    x.AddConsumer<LowStockNotificationConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.UseMessageRetry(r => r.Intervals(500, 1000, 2000, 5000));
        cfg.UsePublishFilter(typeof(CorrelationIdPublishFilter<>), ctx);
        cfg.UseConsumeFilter(typeof(CorrelationIdConsumeFilter<>), ctx);
        cfg.ConfigureEndpoints(ctx);
    });
});

// MassTransit automatically registers its own health checks for RabbitMQ connectivity.
// Tag them as "ready" for Kubernetes readiness probes.
builder.Services.Configure<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckServiceOptions>(options =>
{
    foreach (var reg in options.Registrations)
    {
        reg.Tags.Add("ready");
    }
});

var host = builder.Build();
host.Run();
