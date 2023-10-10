using Service.Common.Abstracttion.Services;
using Service.Common.Infrastructure.Services;
using EventBusRabbitMQ;
using ServiceB.API.IntegrationEvents.EventHandlers;
using EventBus.Bus;
using ServiceB.API.IntegrationEvents.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add Redis cache
{
    builder.Services.AddStackExchangeRedisCache(option =>
    {
        option.Configuration = "192.168.18.227:6379";
        option.InstanceName = "GoFnb";
    });
    builder.Services.AddSingleton<ICacheService, CacheService>();
}

//Add RabbitMQ event bus
{
    var rabbitMQSection = builder.Configuration.GetSection("RabbitMQ");
    if (rabbitMQSection == null)
    {
        throw new ArgumentNullException(nameof(rabbitMQSection));
    }
    builder.Services.AddRabbitMQEventBus
    (
        connectionUrl: rabbitMQSection["ConnectionUrl"],
        brokerName: rabbitMQSection["Broker"],
        queueName: rabbitMQSection["Queue"],
        timeoutBeforeReconnecting: int.Parse(rabbitMQSection["TimeoutBeforeReconnecting"])
    );

    builder.Services.AddTransient<MessageSentEventHandler>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var eventBus = app.Services.GetRequiredService<IEventBus>();

eventBus.Subscribe<MessageSentEvent, MessageSentEventHandler>();

app.Run();