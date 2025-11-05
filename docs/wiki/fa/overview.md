<div dir="rtl" align="right">

# نمای کلی

این الگو شامل Gateway، دو میکروسرویس نمونه (سفارش و موجودی)، لایه Shared، و یک WebUI برای دمو است. تمرکز پروژه روی تاب‌آوری (Polly)، مسیریابی (YARP)، الگوهای معماری (Clean/CQRS)، کش و قفل توزیع‌شده (Redis)، و جلوگیری از تکرار عملیات (Idempotency) است.

## اجزا
- ApiGateway: معبر یکنواخت به سرویس‌ها (YARP + Transform هدرها + Polly)
- OrderService: مدیریت سفارش‌ها (CQRS، AutoMapper، Result)
- InventoryService: مدیریت موجودی (CQRS، Lock/Idempotency)
- Shared: کد مشترک (Result, Headers, Middleware, Cache/Lock, Options)
- WebUI: کلاینت MVC نمونه برای سناریوهای انتها به انتها

## هدرهای قراردادی
- `X-Gateway-Api-Key`: احراز هویت بین Gateway و سرویس‌ها
- `Idempotency-Key`: جلوگیری از اجرای دوباره درخواست‌ها

</div>


