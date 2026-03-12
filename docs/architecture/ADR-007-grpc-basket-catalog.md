# ADR-007: gRPC for Basket → Catalog Internal Communication

## Status

Accepted

## Context

When a user adds an item to their basket, the Basket service must validate that the product exists and capture a price snapshot at that moment. This snapshot is stored in Redis with the basket and later used by the Order service during checkout — ensuring order line items reflect the price at time of cart addition, not the live catalog price.

The question is: how should the Basket service retrieve product data from the Catalog service?

CLAUDE.md (ADR-004) currently specifies that gRPC is used **only** for Order↔Payment and Order↔Notification. All other internal communication defaults to RabbitMQ for async flows.

However, product validation at cart-add time is **synchronous by nature**: the user must receive immediate feedback if the product ID is invalid, and the price snapshot must be captured atomically with the basket update. This rules out asynchronous messaging.

The remaining options for synchronous internal communication are:

1. **REST via Gateway** — Basket calls Catalog through the Gateway's `/api/v1/catalog/products/{id}` endpoint.
2. **REST direct (service-to-service)** — Basket calls Catalog's internal REST API directly using Aspire service discovery.
3. **gRPC direct** — Basket uses a generated gRPC client to call a dedicated Catalog gRPC service.

## Decision

Use **gRPC** for the Basket → Catalog product lookup.

The gRPC contract is defined in `src/Shared/Shared.Grpc/Protos/catalog.proto`. Catalog exposes a `GetProductById` RPC alongside its existing REST API. Basket uses the generated client.

## Consequences

**Positive:**
- Strongly typed contract between services. Schema changes are caught at compile time, not at runtime.
- Better performance than REST for high-frequency internal calls (binary encoding, HTTP/2 multiplexing).
- Consistent with the existing gRPC pattern already used by Order↔Payment.
- `proto` file in `Shared.Grpc` acts as an explicit, versioned contract that both teams own.
- Basket is resilient to Catalog REST API changes (the gRPC surface is separate and minimal).

**Negative:**
- Extends the gRPC surface beyond what ADR-004 initially documented (requires this ADR).
- Adds gRPC server dependency to Catalog.API.
- Adding a new `.proto` file requires rebuilding `Shared.Grpc` before dependent projects compile.

## Alternatives Considered

**REST via Gateway:** Rejected. Routing an internal call through the external Gateway adds unnecessary latency, passes through rate limiting, and creates a circular dependency risk. The Gateway is meant for external clients.

**REST direct (service-to-service):** Viable. Simpler than gRPC (no proto compilation), and Aspire service discovery handles addressing. However, REST lacks compile-time contract enforcement, and the existing codebase already has gRPC infrastructure for internal calls. Using REST here would create inconsistency.

**Async via RabbitMQ (eventual consistency):** Basket could subscribe to `ProductCreatedIntegrationEvent` and `ProductPriceChangedIntegrationEvent` to maintain a local product cache. This is the correct long-term strategy at higher scale (reduces runtime coupling). Rejected for now because: it requires a local product store in Basket, adds projection complexity, and still needs a synchronous fallback for products not yet in cache. This pattern is documented as a future improvement in `docs/architecture/failure-scenarios.md`.

## Future Improvement

If Catalog service availability becomes a concern, consider adding a local product cache in Basket (backed by Redis) populated by consuming `ProductPriceChangedIntegrationEvent` from RabbitMQ. The gRPC call would then serve as a fallback (or be removed entirely). See CLAUDE.md Section 15 — "Catalog Service Unavailable".
