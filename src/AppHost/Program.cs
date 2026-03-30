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
    .WaitFor(catalogDb)
    .WaitFor(elasticsearch)
    .WaitFor(rabbit);

var stockApi = builder.AddProject<Projects.Stock_API>("stock-api")
    .WithReference(stockDb)
    .WithReference(rabbit)
    .WaitFor(stockDb)
    .WaitFor(rabbit);

var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
    .WithReference(redis)
    .WithReference(rabbit)
    .WithReference(catalogApi)
    .WithReference(stockApi)
    .WaitFor(redis)
    .WaitFor(rabbit)
    .WaitFor(catalogApi)
    .WaitFor(stockApi);

var paymentDb = postgres.AddDatabase("payment-db");

var paymentApi = builder.AddProject<Projects.Payment_API>("payment-api")
    .WithReference(paymentDb)
    .WaitFor(paymentDb);
var notificationWorker = builder.AddProject<Projects.Notification_Worker>("notification-worker");

var orderApi = builder.AddProject<Projects.Order_API>("order-api")
    .WithReference(orderDb)
    .WithReference(paymentApi)
    .WithReference(rabbit)
    .WithReference(notificationWorker)
    .WaitFor(orderDb)
    .WaitFor(rabbit);

var gatewayApi = builder.AddProject<Projects.Gateway_API>("gateway-api")
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(orderApi)
    .WithReference(keycloak);

builder.AddNpmApp("react-client", "../client", "dev")
    .WithHttpEndpoint(port: 3000, targetPort: 5173)
    .WithExternalHttpEndpoints()
    .WithReference(gatewayApi)
    .WaitFor(gatewayApi)
    .WaitFor(keycloak);

builder.Build().Run();
