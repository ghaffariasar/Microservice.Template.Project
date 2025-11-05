<div dir="rtl" align="right">

# کش، Idempotency و قفل توزیع‌شده

## کش توزیع‌شده
از طریق `Shared/Extensions/CacheServiceExtensions.AddDistributedCacheAndLock` ثبت می‌شود و با کلید `Cache:Provider` بین `Redis | Memory | SqlServer` سوییچ می‌کند.

اتصال Redis از `ConnectionStrings:Redis` خوانده می‌شود. برای SQL Server از `AddDistributedSqlServerCache` با جدول/اسکیما پیکربندی‌شده استفاده می‌شود.

## قفل توزیع‌شده
اگر Provider = Redis باشد، `DistributedLockService` فعال می‌شود (بر پایه `IConnectionMultiplexer`). در غیر این صورت NoOp Lock ثبت می‌شود.

## Idempotency
سرویس `IIdempotencyService` روی `IDistributedCache` پیاده‌سازی شده است. درخواست‌ها می‌توانند هدر `Idempotency-Key` بفرستند تا از اجرای تکراری جلوگیری شود. نمونه‌ها در Handlerهای ایجاد سفارش و تأیید موجودی قابل مشاهده است.

</div>


