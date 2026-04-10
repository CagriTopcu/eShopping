using Asp.Versioning;
using Scalar.AspNetCore;
using ServiceDefaults;
using Shipping.API.Endpoints;
using Shipping.Application;
using Shipping.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddServiceOpenApi(
    "Shipping API",
    "Shipping and delivery service. Manages shipment lifecycle from order confirmation to delivery.");

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
    options.Title = "Shipping API";
    options.Theme = ScalarTheme.BluePlanet;
    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    options.AddPreferredSecuritySchemes("Bearer");
});

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

var shippingVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

app.MapGroup("/api/v{version:apiVersion}/shipping")
    .WithApiVersionSet(shippingVersionSet)
    .MapToApiVersion(new ApiVersion(1, 0))
    .WithTags("Shipping")
    .RequireAuthorization()
    .MapShippingEndpoints();

app.Run();

public partial class Program;
