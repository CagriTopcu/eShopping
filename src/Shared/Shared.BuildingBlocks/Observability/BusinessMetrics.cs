using System.Diagnostics.Metrics;

namespace Shared.BuildingBlocks.Observability;

/// <summary>
/// Custom business metrics for eShopping.
/// All services share one Meter ("eShopping") so Grafana queries are uniform.
/// </summary>
public sealed class BusinessMetrics : IDisposable
{
    public const string MeterName = "eShopping";

    private readonly Meter _meter;

    // Counters
    private readonly Counter<long> _ordersPlaced;
    private readonly Counter<long> _ordersCancelled;
    private readonly Counter<long> _paymentsFailed;
    private readonly Counter<long> _stockReservationsFailed;
    private readonly Counter<long> _stockLowWarnings;
    private readonly Counter<long> _notificationsSent;

    // Histograms
    private readonly Histogram<double> _orderValue;
    private readonly Histogram<double> _paymentProcessingDuration;
    private readonly Histogram<double> _stockReservationDuration;

    public BusinessMetrics()
    {
        _meter = new Meter(MeterName, "1.0.0");

        _ordersPlaced = _meter.CreateCounter<long>(
            "eshopping.orders.placed",
            unit: "{order}",
            description: "Total number of orders placed");

        _ordersCancelled = _meter.CreateCounter<long>(
            "eshopping.orders.cancelled",
            unit: "{order}",
            description: "Total number of orders cancelled");

        _paymentsFailed = _meter.CreateCounter<long>(
            "eshopping.payments.failed",
            unit: "{payment}",
            description: "Total number of failed payment attempts");

        _stockReservationsFailed = _meter.CreateCounter<long>(
            "eshopping.stock.reservations.failed",
            unit: "{reservation}",
            description: "Total number of failed stock reservations");

        _stockLowWarnings = _meter.CreateCounter<long>(
            "eshopping.stock.low_warnings",
            unit: "{warning}",
            description: "Total number of low stock warnings emitted");

        _notificationsSent = _meter.CreateCounter<long>(
            "eshopping.notifications.sent",
            unit: "{notification}",
            description: "Total notifications sent by type and channel");

        _orderValue = _meter.CreateHistogram<double>(
            "eshopping.orders.value",
            unit: "USD",
            description: "Distribution of order values");

        _paymentProcessingDuration = _meter.CreateHistogram<double>(
            "eshopping.payments.processing.duration",
            unit: "ms",
            description: "Payment processing duration in milliseconds");

        _stockReservationDuration = _meter.CreateHistogram<double>(
            "eshopping.stock.reservation.duration",
            unit: "ms",
            description: "Stock reservation duration in milliseconds");
    }

    public void OrderPlaced(decimal amount, string currency = "USD")
    {
        _ordersPlaced.Add(1, new KeyValuePair<string, object?>("currency", currency));
        _orderValue.Record((double)amount, new KeyValuePair<string, object?>("currency", currency));
    }

    public void OrderCancelled(string reason)
        => _ordersCancelled.Add(1, new KeyValuePair<string, object?>("reason", reason));

    public void PaymentFailed(string reason)
        => _paymentsFailed.Add(1, new KeyValuePair<string, object?>("reason", reason));

    public void StockReservationFailed(string reason)
        => _stockReservationsFailed.Add(1, new KeyValuePair<string, object?>("reason", reason));

    public void StockLowWarning(Guid productId, int remaining)
        => _stockLowWarnings.Add(1,
            new KeyValuePair<string, object?>("product_id", productId.ToString()),
            new KeyValuePair<string, object?>("remaining", remaining));

    public void NotificationSent(string type, string channel)
        => _notificationsSent.Add(1,
            new KeyValuePair<string, object?>("type", type),
            new KeyValuePair<string, object?>("channel", channel));

    public void RecordPaymentDuration(double milliseconds)
        => _paymentProcessingDuration.Record(milliseconds);

    public void RecordStockReservationDuration(double milliseconds)
        => _stockReservationDuration.Record(milliseconds);

    public void Dispose() => _meter.Dispose();
}
