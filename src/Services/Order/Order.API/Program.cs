using Asp.Versioning;
using Order.API.Endpoints;
using Order.Application;
using Order.Infrastructure;
using Scalar.AspNetCore;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddServiceOpenApi(
    "Order API",
    "Order management service. Handles order placement, tracking, and cancellation. Orchestrates payment and stock reservation via saga pattern.");
builder.Services.AddServiceApiVersioning();
builder.Services.AddServiceAuthentication();
builder.Services.AddAuthorizationPolicies();
builder.Services.AddCurrentUser();
builder.Services.AddApplication();
builder.AddInfrastructure();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Order API";
    options.Theme = ScalarTheme.BluePlanet;
    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    options.AddPreferredSecuritySchemes("Bearer");
});
app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

var orderVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

app.MapGroup("/api/v{version:apiVersion}/orders")
    .WithApiVersionSet(orderVersionSet)
    .MapToApiVersion(new ApiVersion(1, 0))
    .WithTags("Order")
    .MapOrderEndpoints();

app.Run();

public partial class Program;
