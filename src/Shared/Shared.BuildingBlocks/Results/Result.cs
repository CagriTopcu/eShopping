namespace Shared.BuildingBlocks.Results;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);

    public static implicit operator Result(Error error) => Failure(error);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);

    public async Task<TResult> MatchAsync<TResult>(
        Func<TValue, Task<TResult>> onSuccess,
        Func<Error, Task<TResult>> onFailure) =>
        IsSuccess ? await onSuccess(Value) : await onFailure(Error);

    public Result<TNew> Map<TNew>(Func<TValue, TNew> mapper) =>
        IsSuccess ? Success(mapper(Value)) : Failure<TNew>(Error);

    public async Task<Result<TNew>> MapAsync<TNew>(Func<TValue, Task<TNew>> mapper) =>
        IsSuccess ? Success(await mapper(Value)) : Failure<TNew>(Error);

    public Result<TNew> Bind<TNew>(Func<TValue, Result<TNew>> binder) =>
        IsSuccess ? binder(Value) : Failure<TNew>(Error);

    public async Task<Result<TNew>> BindAsync<TNew>(Func<TValue, Task<Result<TNew>>> binder) =>
        IsSuccess ? await binder(Value) : Failure<TNew>(Error);

    public Result<TValue> Tap(Action<TValue> action)
    {
        if (IsSuccess) action(Value);
        return this;
    }

    public Result<TValue> TapError(Action<Error> action)
    {
        if (IsFailure) action(Error);
        return this;
    }

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}
