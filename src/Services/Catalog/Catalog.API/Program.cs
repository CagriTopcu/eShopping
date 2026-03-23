using Asp.Versioning;
using Catalog.API.Endpoints;
using Catalog.Application;
using Catalog.Infrastructure;
using Scalar.AspNetCore;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddServiceOpenApi(
    "Catalog API",
    "Product catalog management service. Provides CRUD operations for products, stock adjustments, and search capabilities with filtering and pagination.");
builder.Services.AddServiceApiVersioning();
builder.Services.AddServiceAuthentication();
builder.Services.AddAuthorizationPolicies();
builder.Services.AddCurrentUser();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Catalog API";
    options.Theme = ScalarTheme.BluePlanet;
    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    options.AddPreferredSecuritySchemes("Bearer");
});
app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

var catalogVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

app.MapGroup("/api/v{version:apiVersion}/catalog")
    .WithApiVersionSet(catalogVersionSet)
    .MapToApiVersion(new ApiVersion(1, 0))
    .WithTags("Catalog")
    .MapProductEndpoints();

app.Run();

public partial class Program;
