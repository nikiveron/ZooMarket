using Microsoft.EntityFrameworkCore;
using Order.API.Services;
using Order.Application.Interfaces;
using Order.Domain.Common;
using Order.Infrastructure.Data;
using Order.Infrastructure.Repositories;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrderingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

