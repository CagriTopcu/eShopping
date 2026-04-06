var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", port: 8080)
    .WithRealmImport("./keycloak/eshopping-realm.json")
    .WithDeveloperCertificateTrust(true);

var mongo = builder.AddMongoDB("mongo");
var catalogDb = mongo.AddDatabase("catalog-db");

var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithEnvironment("discovery.type", "single-node")
    .WithEnvironment("xpack.security.enabled", "false");

builder.AddContainer("kibana", "docker.elastic.co/kibana/kibana", "8.17.0")
    .WithHttpEndpoint(port: 5601, targetPort: 5601, name: "ui")
    .WithEnvironment("ELASTICSEARCH_HOSTS", "http://elasticsearch:9200")
    .WaitFor(elasticsearch);

var redis = builder.AddRedis("redis");

var postgres = builder.AddPostgres("postgres");
var stockDb = postgres.AddDatabase("stock-db");
var orderDb = postgres.AddDatabase("order-db");

var rabbit = builder.AddRabbitMQ("rabbitmq");

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(elasticsearch)
    .WithReference(rabbit)
    .WithOtelCollector()
    .WaitFor(catalogDb)
    .WaitFor(elasticsearch)
    .WaitFor(rabbit);

var stockApi = builder.AddProject<Projects.Stock_API>("stock-api")
    .WithReference(stockDb)
    .WithReference(rabbit)
    .WithOtelCollector()
    .WaitFor(stockDb)
    .WaitFor(rabbit);

var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
    .WithReference(redis)
    .WithReference(rabbit)
    .WithReference(catalogApi)
    .WithReference(stockApi)
    .WithOtelCollector()
    .WaitFor(redis)
    .WaitFor(rabbit)
    .WaitFor(catalogApi)
    .WaitFor(stockApi);

var paymentDb = postgres.AddDatabase("payment-db");

var paymentApi = builder.AddProject<Projects.Payment_API>("payment-api")
    .WithReference(paymentDb)
    .WithReference(rabbit)
    .WithOtelCollector()
    .WaitFor(paymentDb)
    .WaitFor(rabbit);
var notificationWorker = builder.AddProject<Projects.Notification_Worker>("notification-worker");

var orderApi = builder.AddProject<Projects.Order_API>("order-api")
    .WithReference(orderDb)
    .WithReference(paymentApi)
    .WithReference(rabbit)
    .WithReference(notificationWorker)
    .WithOtelCollector()
    .WaitFor(orderDb)
    .WaitFor(rabbit);

var gatewayApi = builder.AddProject<Projects.Gateway_API>("gateway-api")
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(orderApi)
    .WithReference(keycloak)
    .WithOtelCollector();

builder.AddNpmApp("react-client", "../client", "dev")
    .WithHttpEndpoint(port: 3000, targetPort: 5173)
    .WithExternalHttpEndpoints()
    .WithReference(gatewayApi)
    .WaitFor(gatewayApi)
    .WaitFor(keycloak);

// ── Observability & Monitoring Stack ──────────────────────────────────

var tempo = builder.AddContainer("tempo", "grafana/tempo", "2.7.2")
    .WithBindMount("./monitoring/tempo/tempo.yaml", "/etc/tempo/tempo.yaml")
    .WithArgs("-config.file=/etc/tempo/tempo.yaml")
    .WithHttpEndpoint(port: 3200, targetPort: 3200, name: "http");

var loki = builder.AddContainer("loki", "grafana/loki", "3.4.2")
    .WithBindMount("./monitoring/loki/loki.yaml", "/etc/loki/loki.yaml")
    .WithArgs("-config.file=/etc/loki/loki.yaml")
    .WithHttpEndpoint(port: 3100, targetPort: 3100, name: "http");

var prometheus = builder.AddContainer("prometheus", "prom/prometheus", "v3.2.1")
    .WithBindMount("./monitoring/prometheus/prometheus.yml", "/etc/prometheus/prometheus.yml")
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

builder.AddContainer("grafana", "grafana/grafana", "11.5.2")
    .WithBindMount("./monitoring/grafana/provisioning", "/etc/grafana/provisioning")
    .WithHttpEndpoint(port: 3300, targetPort: 3000, name: "ui")
    .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
    .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin")
    .WithEnvironment("GF_AUTH_ANONYMOUS_ENABLED", "true")
    .WithEnvironment("GF_AUTH_ANONYMOUS_ORG_ROLE", "Viewer")
    .WaitFor(prometheus)
    .WaitFor(tempo)
    .WaitFor(loki);

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
}
