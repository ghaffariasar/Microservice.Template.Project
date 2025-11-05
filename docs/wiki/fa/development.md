<div dir="rtl" align="right">

# توسعه و اجرا

## پیش‌نیازها
- .NET SDK 9
- Docker (اختیاری برای اجرای سریع Redis و سرویس‌ها)

## اجرا با Docker
در ریشه پروژه:

```bash
docker compose up -d --build
```

## اجرا در IDE
- Solution را باز کرده و پروژه‌های Gateway, OrderService.API, InventoryService.API, WebUI را Start کنید.
- `Gateway:ApiKey` را در همه پروژه‌ها یکسان تنظیم کنید.
- `ApiGatewayUrl` را در `UI/WebUI` مقداردهی کنید.

</div>


