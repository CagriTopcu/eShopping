namespace Basket.Application.Abstractions;

public sealed class CatalogServiceUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);
