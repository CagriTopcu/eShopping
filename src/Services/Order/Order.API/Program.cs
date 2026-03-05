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
app.MapScalarApiReference(options => { options.Title = "Order API"; });
app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

var group = app.MapGroup("/api/v1/orders").WithTags("Order");

group.MapGet("/{username}", (string username) =>
    TypedResults.Ok(new[]
    {
        new OrderResponse(Guid.NewGuid(), username, "Pending", 1359.97m, DateTime.UtcNow)
    }))
    .WithName("GetOrdersByUsername")
    .RequireAuthorization("RequireCustomer");

group.MapPost("/", (CreateOrderRequest request) =>
{
    var id = Guid.NewGuid();
    return TypedResults.Created($"/api/v1/orders/{id}",
        new OrderResponse(id, request.Username, "Pending", request.TotalAmount, DateTime.UtcNow));
})
    .WithName("CreateOrder")
    .RequireAuthorization("RequireCustomer");

app.Run();

record OrderItem(string ProductId, string ProductName, int Quantity, decimal Price);
record OrderResponse(Guid Id, string Username, string Status, decimal TotalAmount, DateTime PlacedAt);
record CreateOrderRequest(string Username, OrderItem[] Items, decimal TotalAmount);
