using Asp.Versioning;
using Payment.API.Endpoints;
using Scalar.AspNetCore;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddServiceOpenApi(
    "Payment API",
    "Payment processing service. Handles payment reservations for order processing. Internal service consumed by order saga orchestration.");
builder.Services.AddServiceApiVersioning();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Payment API";
    options.Theme = ScalarTheme.BluePlanet;
    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
});
app.MapDefaultEndpoints();

var paymentVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

app.MapGroup("/api/v{version:apiVersion}/payments")
    .WithApiVersionSet(paymentVersionSet)
    .MapToApiVersion(new ApiVersion(1, 0))
    .WithTags("Payment")
    .MapPaymentEndpoints();

app.Run();

public partial class Program;
