<div dir="rtl" align="right">

# API Gateway (YARP)

پیکربندی YARP در `src/Gateway/ApiGateway/Program.cs` انجام شده است:

- بارگذاری از `ReverseProxy` در appsettings
- `AddTransforms` برای افزودن هدر `X-Gateway-Api-Key`
- نگاشت مسیرها با `app.MapReverseProxy()`

## Resilience با Polly

روی `HttpClient` نام‌گذاری‌شده `DefaultClient` سیاست‌های Retry ثبت شده است.

## احراز هویت Gateway → سرویس‌ها

در سرویس‌ها، Middleware `UseGatewayAuthentication()` فقط درخواست‌های دارای هدر معتبر را می‌پذیرد (در محیط Development غیرفعال برای سهولت).

</div>


