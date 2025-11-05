# API Gateway (YARP)

Configured in `src/Gateway/ApiGateway/Program.cs`:
- Load routes/clusters from `ReverseProxy` section
- `AddTransforms` to inject `X-Gateway-Api-Key`
- `app.MapReverseProxy()` to serve proxy endpoints

## Resilience (Polly)
`DefaultClient` has Retry policy registered.

## Gateway → Services Auth
`UseGatewayAuthentication()` in services validates `X-Gateway-Api-Key` (disabled in Development).


