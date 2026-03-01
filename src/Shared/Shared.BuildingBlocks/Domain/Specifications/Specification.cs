using System.Linq.Expressions;

namespace Shared.BuildingBlocks.Domain.Specifications;

public abstract class Specification<T> : ISpecification<T>
{
    protected Specification(Expression<Func<T, bool>>? criteria = null)
    {
        Criteria = criteria;
    }

    public Expression<Func<T, bool>>? Criteria { get; }
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    public List<string> IncludeStrings { get; } = [];
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int? Skip { get; private set; }
    public int? Take { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    protected void AddInclude(Expression<Func<T, object>> include) =>
        Includes.Add(include);

    protected void AddInclude(string include) =>
        IncludeStrings.Add(include);

    protected void ApplyOrderBy(Expression<Func<T, object>> orderBy) =>
        OrderBy = orderBy;

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDesc) =>
        OrderByDescending = orderByDesc;

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}
