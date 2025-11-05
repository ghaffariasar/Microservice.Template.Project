using OrderService.Application.Commands;
using OrderService.Application.Mappings;
using OrderService.Infrastructure;
using Serilog;
using Shared.Middleware;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// تنظیمات Logging با Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Order Service API",
        Version = "v1",
        Description = "API برای مدیریت سفارش‌ها - با معماری میکروسرویس",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Order Service",
            Email = "orderservice@example.com"
        }
    });
});

// ثبت MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

// ثبت AutoMapper
builder.Services.AddAutoMapper(typeof(OrderMappingProfile));

// ثبت Infrastructure Services
builder.Services.AddInfrastructure(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API V1");
        c.RoutePrefix = string.Empty; // Swagger در root قرار بگیرد
    });
}

app.UseHttpsRedirection();

// Gateway Authentication - فقط درخواست‌های Gateway را قبول می‌کند
// در Development این Middleware فعال نیست (برای راحتی Development)
app.UseGatewayAuthentication();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information("Starting Order Service API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Order Service API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

