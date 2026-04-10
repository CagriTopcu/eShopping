using System.Net;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Integration.Tests;

/// <summary>
/// Tests that gateway route-level authorization policies work correctly.
/// Uses a fake authentication handler to simulate JWT tokens with various roles,
/// testing against the real YARP routing + auth middleware pipeline.
/// </summary>
public sealed class GatewayAuthTests : IClassFixture<GatewayWebApplicationFactory>
{
    private readonly GatewayWebApplicationFactory _factory;

    public GatewayAuthTests(GatewayWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ── Catalog route: no gateway-level auth (method-agnostic YARP) ─────

    [Theory]
    [InlineData("/api/v1/catalog/products")]
    [InlineData("/api/v1/catalog/products/123")]
    public async Task CatalogRoute_ShouldAllowAnonymousAccess(string path)
    {
        var client = _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync(path);

        // Gateway should forward the request (not 401/403)
        // Downstream service is not running, so we expect 502 Bad Gateway
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    // ── Stock route: RequireAdmin policy ────────────────────────────────

    [Fact]
    public async Task StockRoute_ShouldRejectUnauthenticatedRequests()
    {
        var client = _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync("/api/v1/stock/items");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task StockRoute_ShouldRejectCustomerRole()
    {
        var client = _factory.CreateAuthenticatedClient("Customer");

        var response = await client.GetAsync("/api/v1/stock/items");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task StockRoute_ShouldAllowAdminRole()
    {
        var client = _factory.CreateAuthenticatedClient("Admin");

        var response = await client.GetAsync("/api/v1/stock/items");

        // Admin is allowed through gateway auth; 502 = downstream not available (expected)
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    // ── Basket route: default auth (RequireAuthenticatedUser) ───────────

    [Fact]
    public async Task BasketRoute_ShouldRejectUnauthenticatedRequests()
    {
        var client = _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync("/api/v1/basket/items");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BasketRoute_ShouldAllowAuthenticatedCustomer()
    {
        var client = _factory.CreateAuthenticatedClient("Customer");

        var response = await client.GetAsync("/api/v1/basket/items");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    // ── Order route: default auth (RequireAuthenticatedUser) ────────────

    [Fact]
    public async Task OrderRoute_ShouldRejectUnauthenticatedRequests()
    {
        var client = _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync("/api/v1/orders/my");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OrderRoute_ShouldAllowAuthenticatedUser()
    {
        var client = _factory.CreateAuthenticatedClient("Customer");

        var response = await client.GetAsync("/api/v1/orders/my");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    // ── Profile route: default auth ─────────────────────────────────────

    [Fact]
    public async Task ProfileRoute_ShouldRejectUnauthenticatedRequests()
    {
        var client = _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync("/api/v1/profile/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProfileRoute_ShouldAllowAuthenticatedUser()
    {
        var client = _factory.CreateAuthenticatedClient("Customer");

        var response = await client.GetAsync("/api/v1/profile/me");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    // ── Shipping route: default auth ────────────────────────────────────

    [Fact]
    public async Task ShippingRoute_ShouldRejectUnauthenticatedRequests()
    {
        var client = _factory.CreateUnauthenticatedClient();

        var response = await client.GetAsync("/api/v1/shipping/my-shipments");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShippingRoute_ShouldAllowAuthenticatedUser()
    {
        var client = _factory.CreateAuthenticatedClient("Customer");

        var response = await client.GetAsync("/api/v1/shipping/my-shipments");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}

public sealed class GatewayWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace Keycloak JWT auth with a test scheme
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            // Override the default authentication scheme
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });
        });
    }

    public HttpClient CreateUnauthenticatedClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public HttpClient CreateAuthenticatedClient(params string[] roles)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Comma-separated roles header read by TestAuthHandler
        client.DefaultRequestHeaders.Add("X-Test-Role", string.Join(",", roles));
        client.DefaultRequestHeaders.Add("X-Test-Authenticated", "true");
        return client;
    }
}
