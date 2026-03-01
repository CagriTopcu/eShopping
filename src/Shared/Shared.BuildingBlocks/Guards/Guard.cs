namespace Shared.BuildingBlocks.Guards;

public static class Guard
{
    public static T AgainstNull<T>(T? value, string paramName) where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }

    public static string AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value;
    }

    public static T AgainstDefault<T>(T value, string paramName) where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(value, default))
            throw new ArgumentException("Value cannot be the default.", paramName);
        return value;
    }

    public static int AgainstNegative(int value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, value, "Value cannot be negative.");
        return value;
    }

    public static decimal AgainstNegative(decimal value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, value, "Value cannot be negative.");
        return value;
    }

    public static int AgainstZeroOrNegative(int value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
        return value;
    }

    public static decimal AgainstZeroOrNegative(decimal value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
        return value;
    }
}
