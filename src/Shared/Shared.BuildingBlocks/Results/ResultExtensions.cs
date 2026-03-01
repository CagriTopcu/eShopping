namespace Shared.BuildingBlocks.Results;

public static class ResultExtensions
{
    public static async Task<Result<TNew>> MapAsync<TValue, TNew>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, TNew> mapper) =>
        (await resultTask).Map(mapper);

    public static async Task<Result<TNew>> BindAsync<TValue, TNew>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, Task<Result<TNew>>> binder) =>
        await (await resultTask).BindAsync(binder);

    public static async Task<TResult> MatchAsync<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        (await resultTask).Match(onSuccess, onFailure);

    public static Result<IReadOnlyList<T>> Combine<T>(this IEnumerable<Result<T>> results)
    {
        var resultList = results.ToList();
        var failure = resultList.FirstOrDefault(r => r.IsFailure);

        if (failure is not null)
            return Result.Failure<IReadOnlyList<T>>(failure.Error);

        return Result.Success<IReadOnlyList<T>>(
            resultList.Select(r => r.Value).ToList().AsReadOnly());
    }
}
