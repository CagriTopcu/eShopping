using Scalar.AspNetCore;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddServiceAuthentication();
builder.Services.AddAuthorizationPolicies();
builder.Services.AddCurrentUser();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options => { options.Title = "Catalog API"; });
app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

var group = app.MapGroup("/api/v1/catalog").WithTags("Catalog");

group.MapGet("/products", () =>
    TypedResults.Ok(new[]
    {
        new ProductResponse(Guid.NewGuid(), "Laptop Pro 15", "High-performance laptop", 1299.99m, "Electronics", 50),
        new ProductResponse(Guid.NewGuid(), "Wireless Mouse", "Ergonomic wireless mouse", 29.99m, "Accessories", 200)
    }))
    .WithName("GetProducts")
    .AllowAnonymous();

group.MapGet("/products/{id:guid}", (Guid id) =>
    TypedResults.Ok(new ProductResponse(id, "Laptop Pro 15", "High-performance laptop", 1299.99m, "Electronics", 50)))
    .WithName("GetProductById")
    .AllowAnonymous();

group.MapPost("/products", (CreateProductRequest request) =>
{
    var id = Guid.NewGuid();
    return TypedResults.Created($"/api/v1/catalog/products/{id}",
        new ProductResponse(id, request.Name, request.Description, request.Price, request.Category, request.Stock));
})
    .WithName("CreateProduct")
    .RequireAuthorization("RequireAdmin");

group.MapPut("/products/{id:guid}", (Guid id, CreateProductRequest request) =>
    TypedResults.NoContent())
    .WithName("UpdateProduct")
    .RequireAuthorization("RequireAdmin");

group.MapDelete("/products/{id:guid}", (Guid id) =>
    TypedResults.NoContent())
    .WithName("DeleteProduct")
    .RequireAuthorization("RequireAdmin");

app.Run();

record ProductResponse(Guid Id, string Name, string Description, decimal Price, string Category, int Stock);
record CreateProductRequest(string Name, string Description, decimal Price, string Category, int Stock);
