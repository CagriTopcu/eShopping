using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Pagination;
using Shipping.Application.DTOs;
using Shipping.Domain.Enums;

namespace Shipping.Application.Queries.GetShipments;

public sealed record GetShipmentsQuery(
    ShipmentStatus? Status,
    PaginationParams Pagination) : IQuery<PagedList<ShipmentResponse>>;
