using Basket.API.Endpoints;
using Basket.Application;
using Basket.Infrastructure;
using Scalar.AspNetCore;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddServiceAuthentication();
builder.Services.AddAuthorizationPolicies();
builder.Services.AddCurrentUser();
builder.Services.AddApplication();
builder.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options => { options.Title = "Basket API"; });
app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/v1/basket")
    .WithTags("Basket")
    .MapBasketEndpoints();

app.Run();

public partial class Program;
