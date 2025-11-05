# Caching, Idempotency, and Distributed Lock

## Distributed Cache
Configured via `Shared/Extensions/CacheServiceExtensions.AddDistributedCacheAndLock`. Switch provider with `Cache:Provider` (`Redis | Memory | SqlServer`).

- Redis: `ConnectionStrings:Redis`
- SQL Server: `AddDistributedSqlServerCache` with configured schema/table

## Distributed Lock
With Redis provider, `DistributedLockService` is enabled (uses `IConnectionMultiplexer`). Otherwise a NoOp lock implementation is used.

## Idempotency
`IIdempotencyService` backed by `IDistributedCache`. Clients send `Idempotency-Key` header to prevent duplicate processing. See order creation and product confirmation handlers.


