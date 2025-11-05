# Architecture (Clean Architecture)

Each service follows layers: API (transport), Application (CQRS/MediatR + mapping), Domain (entities/contracts), Infrastructure (EF Core, repositories, DI).

```
Service
 ├─ API
 ├─ Application (Commands, Queries, DTOs, Mappings)
 ├─ Domain (Entities, Repositories Abstractions)
 └─ Infrastructure (EF Core, Repository Impl, DI)
```

Patterns: CQRS, Result Pattern, AutoMapper. Cross-cutting concerns live in `Shared`.


