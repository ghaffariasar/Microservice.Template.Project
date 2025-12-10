using Polly;
using Polly.Extensions.Http;
using Serilog;
using Shared.Configuration;
using Yarp.ReverseProxy.Transforms;
using HeaderNames = Shared.Common.HeaderNames;




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
        Title = "API Gateway",
        Version = "v1",
        Description = "مرکزی برای مدیریت درخواست‌های میکروسرویس‌ها Gateway",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "API Gateway",
            Email = "gateway@example.com"
        }
    });
});

// Bind & validate Gateway options
builder.Services.AddOptions<GatewayOptions>()
    .Bind(builder.Configuration.GetSection("Gateway"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "Gateway:ApiKey is required")
    .ValidateOnStart();

// YARP Reverse Proxy Configuration
var configuration = builder.Configuration;

builder.Services.AddReverseProxy()
    .LoadFromConfig(configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformBuilderContext =>
    {
        // اضافه کردن Header مخصوص Gateway برای Authentication
        var options = transformBuilderContext.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<GatewayOptions>>().Value;
        transformBuilderContext.AddRequestHeader(HeaderNames.GatewayApiKey, options.ApiKey!);
    });



// Polly برای Resilience
builder.Services
    .AddHttpClient("DefaultClient")
    .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));



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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();
app.MapReverseProxy();
app.MapControllers();

try
{
    Log.Information("Starting API Gateway");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}



public partial class Program { }
