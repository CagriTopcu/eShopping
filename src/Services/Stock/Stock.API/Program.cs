using Asp.Versioning;
using Scalar.AspNetCore;
using ServiceDefaults;
using Stock.API.Endpoints;
using Stock.Application;
using Stock.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddServiceOpenApi(
    "Stock API",
    "Stock management service. Tracks product inventory levels. Internal service consumed by catalog and order services for stock operations.");
builder.Services.AddServiceApiVersioning();
builder.Services.AddApplication();
builder.AddInfrastructure();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Stock API";
    options.Theme = ScalarTheme.BluePlanet;
    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
});
app.MapDefaultEndpoints();

var stockVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

app.MapGroup("/api/v{version:apiVersion}/stock")
    .WithApiVersionSet(stockVersionSet)
    .MapToApiVersion(new ApiVersion(1, 0))
    .WithTags("Stock")
    .MapStockEndpoints();

app.Run();

public partial class Program;
