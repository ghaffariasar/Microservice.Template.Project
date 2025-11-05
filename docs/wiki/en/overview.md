# Overview

This template provides an opinionated .NET 9 microservices setup featuring API Gateway (YARP), resilient HTTP (Polly), Clean Architecture, CQRS (MediatR), distributed caching/locking (Redis), idempotency, and structured logging (Serilog).

## Components
- ApiGateway: unified entry (YARP + transforms + Polly)
- OrderService: orders domain (CQRS, AutoMapper, Result)
- InventoryService: stock domain (CQRS, Lock/Idempotency)
- Shared: cross-cutting (Result, Headers, Middleware, Cache/Lock, Options)
- WebUI: demo MVC client

## Contract Headers
- `X-Gateway-Api-Key`: auth between GW and services
- `Idempotency-Key`: idempotent requests


