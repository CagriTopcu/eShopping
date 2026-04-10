using Asp.Versioning;
using Scalar.AspNetCore;
using ServiceDefaults;
using UserProfile.API.Endpoints;
using UserProfile.Application;
using UserProfile.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddServiceOpenApi(
    "User Profile API",
    "User profile and address management service. Stores customer profiles synced with Keycloak identity.");

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
    options.Title = "User Profile API";
    options.Theme = ScalarTheme.BluePlanet;
    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    options.AddPreferredSecuritySchemes("Bearer");
});

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

var profileVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

app.MapGroup("/api/v{version:apiVersion}/profile")
    .WithApiVersionSet(profileVersionSet)
    .MapToApiVersion(new ApiVersion(1, 0))
    .WithTags("Profile")
    .RequireAuthorization()
    .MapProfileEndpoints();

app.Run();

public partial class Program;
