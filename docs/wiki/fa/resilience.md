<div dir="rtl" align="right">

# تاب‌آوری (Polly)

- در `ApiGateway` و `WebUI`، روی `HttpClient` سیاست‌های Retry و CircuitBreaker (در WebUI) ثبت شده‌اند.
- می‌توانید تعداد Retry و Backoff را بر اساس نیاز تغییر دهید.

نمونه (WebUI): `AddHttpClient("DefaultClient").AddPolicyHandler(Retry).AddPolicyHandler(CircuitBreaker)`

</div>


