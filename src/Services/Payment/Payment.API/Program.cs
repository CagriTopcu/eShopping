using Payment.API.Endpoints;
using Scalar.AspNetCore;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options => { options.Title = "Payment API"; });
app.MapDefaultEndpoints();

app.MapGroup("/api/v1/payments")
    .WithTags("Payment")
    .MapPaymentEndpoints();

app.Run();

public partial class Program;
