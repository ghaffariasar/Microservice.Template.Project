<div dir="rtl" align="right">

# معماری و لایه‌ها (Clean Architecture)

ساختار هر سرویس: API (ورودی)، Application (CQRS/MediatR + Mapping + Validation)، Domain (Entities/Contracts)، Infrastructure (EF Core/DbContext/Repositories + Cache/Lock Registration).

```
Service
 ├─ API
 ├─ Application (Commands, Queries, DTOs, Mappings)
 ├─ Domain (Entities, Repositories Abstractions)
 └─ Infrastructure (EF Core, Repository Impl, DI)
```

## الگوها
- CQRS: جداسازی فرمان‌ها و کوئری‌ها با MediatR
- Result Pattern: بازگشت نتیجه یکدست
- AutoMapper: نگاشت شفاف بین Domain ↔ DTO
- Cross-cutting: در `Shared` (هدرها، میان‌افزار Gateway، کش/قفل)

</div>


