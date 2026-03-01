namespace Shared.BuildingBlocks.Domain.Primitives;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
}
