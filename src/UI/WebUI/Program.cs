using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Shared.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Bind & validate WebUI Options
builder.Services.AddOptions<WebUiOptions>()
    .Bind(builder.Configuration)
    .Validate(o => !string.IsNullOrWhiteSpace(o.ApiGatewayUrl), "ApiGatewayUrl is required")
    .ValidateOnStart();

// HTTP Client با Polly برای Resilience (نام: DefaultClient)
builder.Services.AddHttpClient("DefaultClient", (sp, client) =>
{
    var webUiOptions = sp.GetRequiredService<IOptions<WebUiOptions>>().Value;
    client.BaseAddress = new Uri(webUiOptions.ApiGatewayUrl!);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
.AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));


var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

