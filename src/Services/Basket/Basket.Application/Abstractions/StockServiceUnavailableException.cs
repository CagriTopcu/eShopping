namespace Basket.Application.Abstractions;

public sealed class StockServiceUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);
