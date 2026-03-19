using Order.API.Endpoints;
using Order.Application;
using Order.Infrastructure;
using Scalar.AspNetCore;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddServiceAuthentication();
builder.Services.AddAuthorizationPolicies();
builder.Services.AddCurrentUser();
builder.Services.AddApplication();
builder.AddInfrastructure();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options => { options.Title = "Order API"; });
app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/v1/orders")
    .WithTags("Order")
    .MapOrderEndpoints();

app.Run();

public partial class Program;
