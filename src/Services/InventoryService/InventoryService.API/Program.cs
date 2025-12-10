using InventoryService.Application.Commands;
using InventoryService.Application.Mappings;
using InventoryService.Infrastructure;
using Serilog;
using Shared.Middleware;




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
        Title = "Inventory Service API",
        Version = "v1",
        Description = "API برای مدیریت موجودی انبار - با معماری میکروسرویس",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Inventory Service",
            Email = "inventoryservice@example.com"
        }
    });
});

// ثبت MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

// ثبت AutoMapper
builder.Services.AddAutoMapper(typeof(ProductMappingProfile));

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Service API V1");
        c.RoutePrefix = string.Empty;
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
    Log.Information("Starting Inventory Service API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Inventory Service API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


public partial class Program{}
