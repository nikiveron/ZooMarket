using Microsoft.EntityFrameworkCore;
using Order.API.Services;
using Order.Application.Interfaces;
using Order.Domain.Common;
using Order.Domain.Entities;
using Order.Infrastructure.Data;
using Order.Infrastructure.Repositories;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrderingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// RabbitMQ
var rabbitMqHost = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
var rabbitMqPort = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672");
builder.Services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory
{
    HostName = rabbitMqHost,
    Port = rabbitMqPort,
    UserName = "guest",
    Password = "guest"
});
builder.Services.AddSingleton<IConnection>(sp => sp.GetRequiredService<IConnectionFactory>().CreateConnection());
builder.Services.AddSingleton<IModel>(sp =>
{
    var channel = sp.GetRequiredService<IConnection>().CreateModel();
    channel.QueueDeclare("order_events", durable: true, exclusive: false, autoDelete: false);
    return channel;
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI for repositories and services
builder.Services.AddScoped<Order.Domain.Common.IOrderRepository, Order.Infrastructure.Repositories.OrderRepository>();
builder.Services.AddScoped<Order.Application.Interfaces.IOrderService, Order.API.Services.OrderService>();
builder.Services.AddScoped<Order.Application.Interfaces.ICatalogService, Order.API.Services.CatalogServiceStub>();
builder.Services.AddScoped<Order.Application.Interfaces.IPaymentService, Order.API.Services.PaymentServiceStub>();
builder.Services.AddScoped<Order.API.Services.OrderOrchestratorService>();
builder.Services.AddScoped<Order.API.Services.OutboxService>();

// Background service for processing outbox
builder.Services.AddHostedService<OutboxProcessorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();

