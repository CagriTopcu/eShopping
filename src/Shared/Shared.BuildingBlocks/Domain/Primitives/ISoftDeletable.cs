namespace Shared.BuildingBlocks.Domain.Primitives;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
}
