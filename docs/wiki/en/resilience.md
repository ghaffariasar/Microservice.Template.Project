# Resilience (Polly)

- `ApiGateway` and `WebUI` register retry (and circuit breaker in WebUI) policies on `HttpClient`.
- Tune retry counts and backoff as needed.

Example: `AddHttpClient("DefaultClient").AddPolicyHandler(Retry).AddPolicyHandler(CircuitBreaker)`


