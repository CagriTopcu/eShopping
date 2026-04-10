var builder = DistributedApplication.CreateBuilder(args);

var isPublish = builder.ExecutionContext.IsPublishMode;

var keycloak = builder.AddKeycloak("keycloak", port: 8080)
    .WithRealmImport("./keycloak/eshopping-realm.json")
    .WithDeveloperCertificateTrust(true);

var mongo = builder.AddMongoDB("mongo");
var catalogDb = mongo.AddDatabase("catalog-db");

var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithEnvironment("discovery.type", "single-node")
    .WithEnvironment("xpack.security.enabled", "false");

if (!isPublish)
{
    builder.AddContainer("kibana", "docker.elastic.co/kibana/kibana", "8.17.0")
        .WithHttpEndpoint(port: 5601, targetPort: 5601, name: "ui")
        .WithEnvironment("ELASTICSEARCH_HOSTS", "http://elasticsearch:9200")
        .WaitFor(elasticsearch);
}

var redis = builder.AddRedis("redis");

var postgres = builder.AddPostgres("postgres");
var stockDb = postgres.AddDatabase("stock-db");
var orderDb = postgres.AddDatabase("order-db");
var userProfileDb = postgres.AddDatabase("userprofile-db");
var shippingDb = postgres.AddDatabase("shipping-db");

var rabbit = builder.AddRabbitMQ("rabbitmq");

// ── Secret Management ────────────────────────────────────────────────
// Dev-only Vault in dev mode. Production uses external Vault cluster.
if (!isPublish)
{
    builder.AddContainer("vault", "hashicorp/vault", "1.17")
        .WithHttpEndpoint(port: 8200, targetPort: 8200, name: "http")
        .WithEnvironment("VAULT_DEV_ROOT_TOKEN_ID", "dev-root-token")
        .WithEnvironment("VAULT_DEV_LISTEN_ADDRESS", "0.0.0.0:8200")
        .WithArgs("server", "-dev");
}

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(elasticsearch)
    .WithReference(rabbit)
    .WithOtelCollector()
    .WithVault()
    .WaitFor(catalogDb)
    .WaitFor(elasticsearch)
    .WaitFor(rabbit);

var stockApi = builder.AddProject<Projects.Stock_API>("stock-api")
    .WithReference(stockDb)
    .WithReference(rabbit)
    .WithOtelCollector()
    .WithVault()
    .WaitFor(stockDb)
    .WaitFor(rabbit);

var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
    .WithReference(redis)
    .WithReference(rabbit)
    .WithReference(catalogApi) // Service discovery for startup cache warmup only
    .WithOtelCollector()
    .WithVault()
    .WaitFor(redis)
    .WaitFor(rabbit)
    .WaitFor(catalogApi);

var paymentDb = postgres.AddDatabase("payment-db");

var paymentApi = builder.AddProject<Projects.Payment_API>("payment-api")
    .WithReference(paymentDb)
    .WithReference(rabbit)
    .WithOtelCollector()
    .WithVault()
    .WaitFor(paymentDb)
    .WaitFor(rabbit);
var notificationWorker = builder.AddProject<Projects.Notification_Worker>("notification-worker")
    .WithReference(rabbit)
    .WithOtelCollector()
    .WithVault()
    .WaitFor(rabbit);

var orderApi = builder.AddProject<Projects.Order_API>("order-api")
    .WithReference(orderDb)
    .WithReference(rabbit)
    .WithOtelCollector()
    .WithVault()
    .WaitFor(orderDb)
    .WaitFor(rabbit);

var userProfileApi = builder.AddProject<Projects.UserProfile_API>("userprofile-api")
    .WithReference(userProfileDb)
    .WithReference(rabbit)
    .WithOtelCollector()
    .WithVault()
    .WaitFor(userProfileDb)
    .WaitFor(rabbit);

var shippingApi = builder.AddProject<Projects.Shipping_API>("shipping-api")
    .WithReference(shippingDb)
    .WithReference(rabbit)
    .WithOtelCollector()
    .WithVault()
    .WaitFor(shippingDb)
    .WaitFor(rabbit);

var gatewayApi = builder.AddProject<Projects.Gateway_API>("gateway-api")
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(orderApi)
    .WithReference(stockApi)
    .WithReference(userProfileApi)
    .WithReference(shippingApi)
    .WithReference(keycloak)
    .WithOtelCollector();

if (!isPublish)
{
    builder.AddNpmApp("react-client", "../client", "dev")
        .WithHttpEndpoint(port: 3000, targetPort: 5173)
        .WithExternalHttpEndpoints()
        .WithReference(gatewayApi)
        .WaitFor(gatewayApi)
        .WaitFor(keycloak);
}

// ── Observability & Monitoring Stack ──────────────────────────────────
// Dev-only: full Grafana stack is provisioned locally via Aspire.
// In publish/production mode, these are expected to be provided externally.

if (!isPublish)
{
    var tempo = builder.AddContainer("tempo", "grafana/tempo", "2.7.2")
        .WithBindMount("./monitoring/tempo/tempo.yaml", "/etc/tempo/tempo.yaml")
        .WithArgs("-config.file=/etc/tempo/tempo.yaml")
        .WithHttpEndpoint(port: 3200, targetPort: 3200, name: "http");

    var loki = builder.AddContainer("loki", "grafana/loki", "3.4.2")
        .WithBindMount("./monitoring/loki/loki.yaml", "/etc/loki/loki.yaml")
        .WithArgs("-config.file=/etc/loki/loki.yaml")
        .WithHttpEndpoint(port: 3100, targetPort: 3100, name: "http");

    var prometheus = builder.AddContainer("prometheus", "prom/prometheus", "v3.2.1")
        .WithBindMount("./monitoring/prometheus", "/etc/prometheus")
        .WithArgs(
            "--config.file=/etc/prometheus/prometheus.yml",
            "--web.enable-remote-write-receiver",
            "--enable-feature=native-histograms",
            "--storage.tsdb.retention.time=15d")
        .WithHttpEndpoint(port: 9090, targetPort: 9090, name: "http");

    var otelCollector = builder.AddContainer("otel-collector", "otel/opentelemetry-collector-contrib", "0.120.0")
        .WithBindMount("./monitoring/otel-collector/otel-collector-config.yaml", "/etc/otelcol/config.yaml")
        .WithArgs("--config=/etc/otelcol/config.yaml")
        .WithEndpoint(port: 4327, targetPort: 4317, name: "otlp-grpc", scheme: "http")
        .WithHttpEndpoint(port: 4328, targetPort: 4318, name: "otlp-http")
        .WaitFor(tempo)
        .WaitFor(loki)
        .WaitFor(prometheus);

    // Grafana credentials: sourced from environment or defaults for dev.
    // In production, set GF_ADMIN_USER and GF_ADMIN_PASSWORD env vars (from Vault/CI).
    var grafanaUser = builder.Configuration["GF_ADMIN_USER"] ?? "admin";
    var grafanaPassword = builder.Configuration["GF_ADMIN_PASSWORD"] ?? "admin";

    builder.AddContainer("grafana", "grafana/grafana", "11.5.2")
        .WithBindMount("./monitoring/grafana/provisioning", "/etc/grafana/provisioning")
        .WithHttpEndpoint(port: 3300, targetPort: 3000, name: "ui")
        .WithEnvironment("GF_SECURITY_ADMIN_USER", grafanaUser)
        .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", grafanaPassword)
        .WithEnvironment("GF_AUTH_ANONYMOUS_ENABLED", "true")
        .WithEnvironment("GF_AUTH_ANONYMOUS_ORG_ROLE", "Viewer")
        .WaitFor(prometheus)
        .WaitFor(tempo)
        .WaitFor(loki);
}

builder.Build().Run();

// ── Extensions ───────────────────────────────────────────────────────

static class AppHostExtensions
{
    /// <summary>
    /// Configures OTel Collector endpoints for traces/metrics (gRPC) and logs (HTTP).
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithOtelCollector(
        this IResourceBuilder<ProjectResource> resource) => resource
        .WithEnvironment("OTEL_COLLECTOR_ENDPOINT", "http://localhost:4327")
        .WithEnvironment("OTEL_COLLECTOR_HTTP_ENDPOINT", "http://localhost:4328")
        .WithEnvironment("LOKI_OTLP_ENDPOINT", "http://localhost:3100/otlp");

    /// <summary>
    /// Configures Vault connectivity for secret management.
    /// In dev mode, uses the dev root token; production should use AppRole or Kubernetes auth.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithVault(
        this IResourceBuilder<ProjectResource> resource) => resource
        .WithEnvironment("VAULT_URL", "http://localhost:8200")
        .WithEnvironment("VAULT_TOKEN", "dev-root-token")
        .WithEnvironment("VAULT_MOUNT_POINT", "secret");
}
