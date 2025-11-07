<div dir="rtl" align="right">

<h2>پروژه میکروسرویس (دات نت 9)</h2>

این مخزن یک الگوی کامل برای ساخت سیستم‌های میکروسرویس بر پایه .NET 9 است که با معماری تمیز (Clean Architecture) و الگوهای مدرن پیاده‌سازی شده است. مهم‌ترین تکنولوژی‌ها و ابزارهای استفاده‌شده:
</div>

- <b>Polly</b>: تاب‌آوری و سیاست‌های Retry/Circuit Breaker برای HttpClient
- <b>YARP</b>: پیاده‌سازی API Gateway با Reverse Proxy و Transformها
- <b>AutoMapper</b>: نگاشت موجودیت‌ها به DTO و بالعکس
- <b>Result Pattern</b>: الگوی نتیجه برای مدیریت موفق/ناموفق بودن عملیات
- <b>Redis Cache</b>: کش توزیع‌شده (قابل سوییچ به Memory/SQL Server)
- <b>Idempotency Key</b>: جلوگیری از اجرای تکراری عملیات حساس
- <b>Serilog</b>: لاگ ساخت‌یافته و قابل‌گسترش
- <b>Clean Architecture</b>: جداسازی لایه‌ها و حداقل کوپلینگ
- <b>CQRS</b>: جداسازی فرمان‌ها و کوئری‌ها با MediatR
- <b>Distributed Lock</b>: قفل توزیع‌شده مبتنی بر Redis برای سناریوهای رقابتی

<div dir="rtl" align="right">

### ساختار پروژه
</div>


```
src/
  Gateway/ApiGateway              → API Gateway (YARP + Polly + Serilog)
  Services/
    InventoryService/             → سرویس موجودی (API, Application, Domain, Infrastructure)
    OrderService/                 → سرویس سفارش (API, Application, Domain, Infrastructure)
  Shared/Shared                   → کدهای مشترک (Result, Headers, Middleware, Cache/Lock, Options)
UI/WebUI                          → وب‌اپ ساده MVC برای دمو (HttpClient + Polly)
```

<div dir="rtl" align="right">
  
### پیکربندی‌های کلیدی

- هدرهای سفارشی در `Shared/Common/HeaderNames.cs`:
-  `X-Gateway-Api-Key` برای احراز هویت بین Gateway و سرویس‌ها
- `Idempotency-Key` برای سناریوهای عدم تکرار
- انتخاب Provider کش در `appsettings`: کلید `Cache:Provider` یکی از `Redis | Memory | SqlServer`
- اتصال Redis در `ConnectionStrings:Redis`
- گزینه‌های Gateway در `GatewayOptions` (کلید `Gateway:ApiKey` اجباری)


### اجرای سریع (Docker)

1) پیش‌نیاز: Docker Desktop
2) در ریشه پروژه:

</div>


```bash
docker compose up -d --build
```

- Api Gateway: http://localhost:5000
- Order Service (Swagger): http://localhost:<port>
- Inventory Service (Swagger): http://localhost:<port>
- WebUI: http://localhost:5100 (نمونه کلاینت)


<div dir="rtl">

  
### نکات مهم پیاده‌سازی

- <b>Polly</b>:
<div align="right">
در `ApiGateway` و `WebUI` روی `HttpClient` با Retry/CircuitBreaker ثبت شده است.
</div>

- <b>YARP</b>:
<div align="right">
در `ApiGateway/Program.cs` با `AddReverseProxy().LoadFromConfig(...).AddTransforms(...)` و `app.MapReverseProxy()` فعال است.
</div>

- <b>AutoMapper</b>:
<div align="right">
پروفایل‌ها در `*.Application/Mappings/*MappingProfile.cs` ثبت شده‌اند.
</div>

- <b>Result Pattern</b>:
<div align="right">
در `Shared/Common/Result.cs` تعریف و در Handlerها استفاده می‌شود.
</div>

- <b>Cache & Lock</b>:
<div align="right">
اکستنشن `AddDistributedCacheAndLock` در `Shared/Extensions/CacheServiceExtensions.cs` با قابلیت سوییچ بین Redis/Memory/SQLServer.
</div>

- <b>Idempotency</b>:
<div align="right">
سرویس `IIdempotencyService` با `IDistributedCache` پیاده‌سازی شده است؛ هدر `Idempotency-Key` پذیرفته می‌شود.
</div>

- <b>Serilog</b>:
<div align="right">
در تمام `Program.cs`ها با `UseSerilog()` و `WriteTo.Console()` پیکربندی شده است.
</div>

- <b>CQRS</b>
<div align="right">
فرمان‌ها/کوئری‌ها با MediatR در لایه Application هر سرویس قرار دارند.
</div>

- <b>Distributed Lock</b>:
<div align="right">
پیاده‌سازی Redis-based در `Shared/Services/DistributedLockService.cs`، در سناریوهای رزرو/تأیید موجودی استفاده می‌شود.
</div>




### توسعه محلی

- اجرای Solution از طریق Visual Studio/Rider و انتخاب استارت هم‌زمان پروژه‌ها (Gateway, Services, WebUI)
- تنظیم `ApiGatewayUrl` برای WebUI در `appsettings.json`
- تنظیم `Gateway:ApiKey` و مقداردهی همان مقدار در Transform هدر YARP


### مستندات بیشتر

صفحات ویکی در مسیر `docs/wiki` (فارسی و انگلیسی) شامل:
- نمای کلی، معماری، سرویس‌ها، Gateway، Resilience با Polly، کش/Idempotency/Lock، لاگینگ، CQRS و معماری تمیز، توسعه و پیکربندی.

<hr/>


</div>


## Microservice Template (.NET 9)

This repository is a complete template for building microservices on .NET 9 using Clean Architecture and modern production-ready patterns:

- Polly: resilience policies (Retry/Circuit Breaker) for HttpClient
- YARP: API Gateway with Reverse Proxy and transforms
- AutoMapper: mapping entities to DTOs
- Result Pattern: unified success/failure results
- Redis Cache: distributed caching (switchable to Memory/SQL Server)
- Idempotency Key: preventing duplicate execution of critical operations
- Serilog: structured logging
- Clean Architecture: layered, decoupled design
- CQRS: commands/queries with MediatR
- Distributed Lock: Redis-based lock for race-prone scenarios

### Project layout

```
src/
  Gateway/ApiGateway              → API Gateway (YARP + Polly + Serilog)
  Services/
    InventoryService/             → Inventory service (API, Application, Domain, Infrastructure)
    OrderService/                 → Order service (API, Application, Domain, Infrastructure)
  Shared/Shared                   → Shared code (Result, Headers, Middleware, Cache/Lock, Options)
UI/WebUI                          → Simple MVC demo client (HttpClient + Polly)
```

### Key configuration

- Headers in `Shared/Common/HeaderNames.cs`:
  - `X-Gateway-Api-Key` for GW-to-service auth
  - `Idempotency-Key` for idempotent ops
- Cache provider: `Cache:Provider` = `Redis | Memory | SqlServer`
- Redis connection: `ConnectionStrings:Redis`
- Gateway options: `Gateway:ApiKey` (required)

### Quick start (Docker)

```bash
docker compose up -d --build
```

- API Gateway: http://localhost:5000
- Order/Inventory Swagger: per container mapping
- WebUI: http://localhost:5100

### Implementation highlights

- Polly on `HttpClient` in `ApiGateway` and `WebUI`
- YARP configured in `ApiGateway/Program.cs` with transforms and `MapReverseProxy`
- AutoMapper profiles under `*.Application/Mappings`
- Result Pattern in `Shared/Common/Result.cs`
- Cache & Lock switch via `Shared/Extensions/CacheServiceExtensions.cs`
- Idempotency backed by `IDistributedCache`; header `Idempotency-Key`
- Serilog with `UseSerilog()` and console sink
- CQRS via MediatR in Application layer
- Distributed Lock via Redis in `Shared/Services/DistributedLockService.cs`

### More docs

See `docs/wiki` for detailed FA/EN pages on architecture, services, gateway, resilience, caching/idempotency/lock, logging, CQRS, development, and configuration.


