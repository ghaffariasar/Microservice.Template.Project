# Development & Run

## Prereqs
- .NET SDK 9
- Docker (optional for Redis and quick run)

## Docker

```bash
docker compose up -d --build
```

## IDE
- Start ApiGateway, OrderService.API, InventoryService.API, WebUI
- Keep `Gateway:ApiKey` consistent
- Set `WebUI:ApiGatewayUrl`


