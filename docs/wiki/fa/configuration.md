<div dir="rtl" align="right">

# پیکربندی

نمونه کلیدهای مهم در `appsettings.json`:

```json
{
  "Gateway": { "ApiKey": "YOUR-GATEWAY-KEY" },
  "Cache": {
    "Provider": "Redis", // یا Memory یا SqlServer
    "Redis": { "ConnectionString": "localhost:6379" },
    "SqlServer": { "SchemaName": "dbo", "TableName": "Cache" }
  },
  "ConnectionStrings": {
    "Redis": "localhost:6379",
    "DefaultConnection": "Server=.;Database=AppDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "WebUI": { "ApiGatewayUrl": "http://localhost:5000" }
}
```

</div>


