# ADR-007: Gateway-Level OpenAPI Aggregation for Unified API Documentation

## Status
Accepted

## Context
The Gateway is the sole external entry point for all API consumers. Each downstream
service exposes its own OpenAPI spec at /openapi/v1.json and a Scalar UI at /scalar/v1.
An external client or developer using the Gateway address has no visibility into the full
set of available endpoints: the Gateway's own Scalar UI only reflects the /health
endpoint the Gateway registers directly, not the proxied routes.

This creates a documentation gap. A consumer who points their HTTP client generation
tool or Scalar at the Gateway URL receives an incomplete spec. Individual service ports
are internal concerns that must not be exposed to external consumers in production, so
pointing consumers at each service directly violates the single-entry-point principle.

Three forces are in play:
1. The Gateway is the only legitimate external endpoint per architecture principle.
2. All downstream services already produce well-formed OpenAPI 3.x specs via
   Microsoft.AspNetCore.OpenApi.
3. No new NuGet packages are introduced; the solution uses System.Text.Json (in-box)
   and IHttpClientFactory already registered by ServiceDefaults.

## Decision
The Gateway aggregates downstream OpenAPI specs on first request using a lazy-initialized
in-memory cache (one fetch per process lifetime, refreshable via ?refresh=true).

OpenApiAggregator fetches /openapi/v1.json from each downstream service using named
HttpClients that have Aspire service discovery and Polly resilience attached via
ServiceDefaults. The JSON responses are merged with System.Text.Json: paths objects are
concatenated (no prefix conflicts because each service owns a distinct path prefix) and
components/schemas objects are merged with a service-tag prefix on collision.

The merged spec is served at /openapi/v1.json on the Gateway, replacing ASP.NET Core's
default generated spec. Scalar at /scalar/v1 continues to read from /openapi/v1.json
and therefore shows the full aggregated surface.

Individual service Scalar UIs remain intact at their own ports for internal development.

## Consequences
Positive:
- External clients and tooling pointed at the Gateway see the complete API surface.
- No new package dependencies.
- Downstream services require no changes.
- Failing services are skipped silently; a partial spec is served with a warning log.
- ?refresh=true allows cache busting after a service redeploy without Gateway restart.

Negative:
- The spec is stale for the process lifetime. A new endpoint added to a service requires
  a Gateway restart (or ?refresh=true) to appear. Acceptable because endpoint additions
  require a service redeploy anyway.
- Schema name collisions across services are resolved by prefixing with the service tag.
  Services must use distinct type names to avoid this; they currently do.

## Alternatives Considered
1. Redirect clients to individual service Scalar UIs.
   Rejected: violates the single-entry-point principle and exposes internal ports.

2. Third-party aggregation package (Swashbuckle, NSwag).
   Rejected: introduces new package dependencies; Swashbuckle is incompatible with
   Minimal APIs' OpenAPI pipeline.

3. Static spec committed to the repository.
   Rejected: becomes stale immediately; requires a manual process on every endpoint
   change.

4. Separate paths per service (/openapi/catalog.json, /openapi/basket.json).
   Rejected: requires more complex client configuration; a single /openapi/v1.json
   matches what code-generation tools expect.
