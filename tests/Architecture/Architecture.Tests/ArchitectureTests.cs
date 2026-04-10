using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace Architecture.Tests;

/// <summary>
/// Architectural fitness tests that enforce layer boundaries, naming conventions,
/// and structural rules across all eShopping microservices.
/// </summary>
public sealed class ArchitectureTests
{
    // ── Assembly references for each layer ───────────────────────────────

    private static readonly Assembly[] DomainAssemblies =
    [
        typeof(Catalog.Domain.Entities.Product).Assembly,
        typeof(Order.Domain.Entities.Order).Assembly,
        typeof(Stock.Domain.Entities.StockItem).Assembly,
        typeof(Payment.Domain.Entities.Payment).Assembly,
        typeof(Basket.Domain.Entities.Basket).Assembly,
        typeof(Shipping.Domain.Entities.Shipment).Assembly,
        typeof(UserProfile.Domain.Entities.Profile).Assembly
    ];

    private static readonly Assembly[] ApplicationAssemblies =
    [
        typeof(Catalog.Application.Commands.CreateProduct.CreateProductCommand).Assembly,
        typeof(Order.Application.Sagas.OrderSaga).Assembly,
        typeof(Stock.Application.Consumers.ReserveStockConsumer).Assembly,
        typeof(Payment.Application.Consumers.ProcessPaymentConsumer).Assembly,
        typeof(Basket.Application.Commands.UpsertBasketItem.UpsertBasketItemCommand).Assembly,
        typeof(Shipping.Application.Consumers.OrderConfirmedConsumer).Assembly,
        typeof(UserProfile.Application.Commands.CreateProfile.CreateProfileCommand).Assembly
    ];

    private static readonly Assembly[] InfrastructureAssemblies =
    [
        typeof(Catalog.Infrastructure.DependencyInjection).Assembly,
        typeof(Order.Infrastructure.DependencyInjection).Assembly,
        typeof(Stock.Infrastructure.DependencyInjection).Assembly,
        typeof(Payment.Infrastructure.DependencyInjection).Assembly,
        typeof(Basket.Infrastructure.DependencyInjection).Assembly,
        typeof(Shipping.Infrastructure.DependencyInjection).Assembly,
        typeof(UserProfile.Infrastructure.DependencyInjection).Assembly
    ];

    // ── Layer dependency rules ──────────────────────────────────────────
    // Domain must NOT depend on Infrastructure or Application

    [Theory]
    [MemberData(nameof(GetDomainAssemblies))]
    public void Domain_ShouldNotDependOnInfrastructure(Assembly domainAssembly)
    {
        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Catalog.Infrastructure",
                "Order.Infrastructure",
                "Stock.Infrastructure",
                "Payment.Infrastructure",
                "Basket.Infrastructure",
                "Shipping.Infrastructure",
                "UserProfile.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain layer {domainAssembly.GetName().Name} must not depend on Infrastructure. " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    [Theory]
    [MemberData(nameof(GetDomainAssemblies))]
    public void Domain_ShouldNotDependOnApplication(Assembly domainAssembly)
    {
        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Catalog.Application",
                "Order.Application",
                "Stock.Application",
                "Payment.Application",
                "Basket.Application",
                "Shipping.Application",
                "UserProfile.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain layer {domainAssembly.GetName().Name} must not depend on Application. " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    // Application must NOT depend on Infrastructure

    [Theory]
    [MemberData(nameof(GetApplicationAssemblies))]
    public void Application_ShouldNotDependOnInfrastructure(Assembly appAssembly)
    {
        var result = Types.InAssembly(appAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Catalog.Infrastructure",
                "Order.Infrastructure",
                "Stock.Infrastructure",
                "Payment.Infrastructure",
                "Basket.Infrastructure",
                "Shipping.Infrastructure",
                "UserProfile.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Application layer {appAssembly.GetName().Name} must not depend on Infrastructure. " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    // Domain must NOT depend on external frameworks (EF, MassTransit, ASP.NET)

    [Theory]
    [MemberData(nameof(GetDomainAssemblies))]
    public void Domain_ShouldNotDependOnExternalFrameworks(Assembly domainAssembly)
    {
        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "MassTransit",
                "Npgsql",
                "MongoDB.Driver")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain layer {domainAssembly.GetName().Name} must not depend on external frameworks. " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    // ── Naming conventions ──────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(GetAllAssemblies))]
    public void Consumers_ShouldEndWithConsumer(Assembly assembly)
    {
        var result = Types.InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(MassTransit.IConsumer<>))
            .Should()
            .HaveNameEndingWith("Consumer")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"All MassTransit consumers must end with 'Consumer'. " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    // ── Structural rules ────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(GetAllAssemblies))]
    public void Consumers_ShouldBeSealed(Assembly assembly)
    {
        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Consumer")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"All consumers must be sealed for performance. " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    [Theory]
    [MemberData(nameof(GetDomainAssemblies))]
    public void DomainEntities_ShouldBeSealed(Assembly domainAssembly)
    {
        var result = Types.InAssembly(domainAssembly)
            .That()
            .ResideInNamespaceEndingWith(".Entities")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain entities in {domainAssembly.GetName().Name} must be sealed. " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    [Theory]
    [MemberData(nameof(GetDomainAssemblies))]
    public void DomainEvents_ShouldBeSealed(Assembly domainAssembly)
    {
        var result = Types.InAssembly(domainAssembly)
            .That()
            .ResideInNamespaceEndingWith(".Events")
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain events in {domainAssembly.GetName().Name} must be sealed. " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    // ── Integration events structural rules ─────────────────────────────

    [Fact]
    public void IntegrationEvents_ShouldBeSealed()
    {
        var contractsAssembly = typeof(Shared.Contracts.Events.IntegrationEvent).Assembly;

        var result = Types.InAssembly(contractsAssembly)
            .That()
            .Inherit(typeof(Shared.Contracts.Events.IntegrationEvent))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"All integration events must be sealed. " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void IntegrationEvents_ShouldEndWithEvent()
    {
        var contractsAssembly = typeof(Shared.Contracts.Events.IntegrationEvent).Assembly;

        var result = Types.InAssembly(contractsAssembly)
            .That()
            .Inherit(typeof(Shared.Contracts.Events.IntegrationEvent))
            .Should()
            .HaveNameEndingWith("Event")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"All integration events must end with 'Event'. " +
            $"Violating types: {FormatFailingTypes(result)}");
    }

    // ── Test data providers ─────────────────────────────────────────────

    public static TheoryData<Assembly> GetDomainAssemblies()
    {
        var data = new TheoryData<Assembly>();
        foreach (var a in DomainAssemblies) data.Add(a);
        return data;
    }

    public static TheoryData<Assembly> GetApplicationAssemblies()
    {
        var data = new TheoryData<Assembly>();
        foreach (var a in ApplicationAssemblies) data.Add(a);
        return data;
    }

    public static TheoryData<Assembly> GetAllAssemblies()
    {
        var data = new TheoryData<Assembly>();
        foreach (var a in DomainAssemblies.Concat(ApplicationAssemblies).Concat(InfrastructureAssemblies))
            data.Add(a);
        return data;
    }

    private static string FormatFailingTypes(TestResult result)
    {
        if (result.FailingTypes is null || result.FailingTypes.Count == 0)
            return "(none)";
        return string.Join(", ", result.FailingTypes.Select(t => t.FullName));
    }
}
