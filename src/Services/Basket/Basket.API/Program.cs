using Asp.Versioning;
using Basket.API.Endpoints;
using Basket.Application;
using Basket.Infrastructure;
using Scalar.AspNetCore;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddServiceOpenApi(
    "Basket API",
    "Shopping basket management service. Handles adding, updating, and removing items from user shopping baskets with real-time price calculation.");
builder.Services.AddServiceApiVersioning();
builder.Services.AddServiceAuthentication();
builder.Services.AddAuthorizationPolicies();
builder.Services.AddCurrentUser();
builder.Services.AddApplication();
builder.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Basket API";
    options.Theme = ScalarTheme.BluePlanet;
    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    options.AddPreferredSecuritySchemes("Bearer");
});
app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

var basketVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

app.MapGroup("/api/v{version:apiVersion}/basket")
    .WithApiVersionSet(basketVersionSet)
    .MapToApiVersion(new ApiVersion(1, 0))
    .WithTags("Basket")
    .MapBasketEndpoints();

app.Run();

public partial class Program;
