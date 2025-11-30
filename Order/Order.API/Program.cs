using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Order.API.Services;
using Order.Application.Interfaces;
using Order.Infrastructure.Data;
using RabbitMQ.Client;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrderingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));


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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Order API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Entry JWT token: Bearer {token}",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ZoomarketIdentity";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ZoomarketServices";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// DI for repositories and services
builder.Services.AddScoped<Order.Domain.Common.IOrderRepository, Order.Infrastructure.Repositories.OrderRepository>();
builder.Services.AddScoped<Order.Application.Interfaces.IOrderService, Order.API.Services.OrderService>();
builder.Services.AddScoped<Order.Application.Interfaces.ICatalogService, Order.API.Services.CatalogServiceStub>();
builder.Services.AddScoped<Order.Application.Interfaces.IPaymentService, Order.API.Services.PaymentServiceStub>();
builder.Services.AddScoped<ICartService, CartService>();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();

