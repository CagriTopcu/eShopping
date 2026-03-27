using Catalog.Application.Abstractions;
using Catalog.Domain.Entities;
using Mapster;
using MassTransit;
using MediatR;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;
using Shared.Contracts.Events.Catalog;
using Catalog.Application.ReadModels;

namespace Catalog.Application.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IProductWriteRepository writeRepository,
    IProductReadRepository readRepository,
    IPublisher publisher,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var result = Product.Create(
            request.Name,
            request.Price,
            request.Currency,
            request.Category,
            request.Stock,
            request.Description,
            request.ImageUrl);

        if (result.IsFailure)
            return result.Error;

        var product = result.Value;

        await writeRepository.AddAsync(product, cancellationToken);
        await readRepository.UpsertAsync(product.Adapt<ProductReadModel>(), cancellationToken);

        foreach (var domainEvent in product.DomainEvents)
            await publisher.Publish(domainEvent, cancellationToken);

        product.ClearDomainEvents();

        await publishEndpoint.Publish(
            new ProductCreatedIntegrationEvent(
                product.Id.Value,
                product.Name.Value,
                product.Category.Name,
                product.Price.Amount,
                product.Price.Currency,
                product.Stock.Value),
            cancellationToken);

        return product.Id.Value;
    }
}
