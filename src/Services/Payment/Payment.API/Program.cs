using Asp.Versioning;
using Payment.API.Endpoints;
using Payment.Application;
using Payment.Infrastructure;
using Scalar.AspNetCore;
using ServiceDefaults;
using Shared.BuildingBlocks.Idempotency;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddIdempotencySupport();
builder.Services.AddServiceOpenApi(
    "Payment API",
    "Payment processing service. Handles payment reservations, captures, and refunds for order processing.");
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
    options.Title = "Payment API";
    options.Theme = ScalarTheme.BluePlanet;
    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    options.AddPreferredSecuritySchemes("Bearer");
});
app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

var paymentVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

app.MapGroup("/api/v{version:apiVersion}/payments")
    .WithApiVersionSet(paymentVersionSet)
    .MapToApiVersion(new ApiVersion(1, 0))
    .WithTags("Payment")
    .RequireAuthorization()
    .MapPaymentEndpoints();

app.Run();

public partial class Program;
