# Configuration

Key appsettings:

```json
{
  "Gateway": { "ApiKey": "YOUR-GATEWAY-KEY" },
  "Cache": {
    "Provider": "Redis",
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


