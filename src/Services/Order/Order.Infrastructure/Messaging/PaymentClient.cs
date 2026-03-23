using Order.Application.Abstractions;
using System.Net.Http.Json;

namespace Order.Infrastructure.Messaging;

internal sealed class PaymentClient(HttpClient httpClient) : IPaymentClient
{
    private const string ApiVersion = "v1";

    public async Task<bool> ReserveAsync(Guid orderId, Guid customerId, decimal amount, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"/api/{ApiVersion}/payments/reserve",
            new { orderId, customerId, amount, currency = "USD" },
            ct);

        return response.IsSuccessStatusCode;
    }
}
