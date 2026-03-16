using Scalar.AspNetCore;
using ServiceDefaults;
using Stock.API.Endpoints;
using Stock.Application;
using Stock.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.AddInfrastructure();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options => { options.Title = "Stock API"; });
app.MapDefaultEndpoints();

app.MapGroup("/api/v1/stock")
    .WithTags("Stock")
    .MapStockEndpoints();

app.Run();

public partial class Program;
