using System.Collections.Frozen;
using System.Text;
using System.Text.Json;

namespace Gateway.API.OpenApi;

internal sealed class OpenApiAggregator
{
    private static readonly FrozenDictionary<string, string> ServiceBaseAddresses =
        new Dictionary<string, string>
        {
            ["catalog"] = "http://catalog-api",
            ["basket"]  = "http://basket-api",
            ["orders"]  = "http://order-api"
        }.ToFrozenDictionary();

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenApiAggregator> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _cachedSpec;

    public OpenApiAggregator(
        IHttpClientFactory httpClientFactory,
        ILogger<OpenApiAggregator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> GetAggregatedSpecAsync(
        bool refresh = false,
        CancellationToken ct = default)
    {
        if (!refresh && _cachedSpec is not null)
            return _cachedSpec;

        await _lock.WaitAsync(ct);
        try
        {
            if (!refresh && _cachedSpec is not null)
                return _cachedSpec;

            _cachedSpec = await BuildMergedSpecAsync(ct);
            return _cachedSpec;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<string> BuildMergedSpecAsync(CancellationToken ct)
    {
        var specs = new List<(string Tag, JsonDocument Doc)>();

        foreach (var (tag, baseAddress) in ServiceBaseAddresses)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(tag);
                using var response = await client.GetAsync(
                    $"{baseAddress}/openapi/v1.json", ct);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync(ct);
                var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                specs.Add((tag, doc));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to fetch OpenAPI spec from {Service}. Omitting from aggregated spec.",
                    tag);
            }
        }

        var result = MergeSpecs(specs);
        foreach (var (_, doc) in specs) doc.Dispose();
        return result;
    }

    private static string MergeSpecs(List<(string Tag, JsonDocument Doc)> specs)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

        writer.WriteStartObject();
        writer.WriteString("openapi", "3.0.1");

        writer.WriteStartObject("info");
        writer.WriteString("title", "eShopping API");
        writer.WriteString("version", "v1");
        writer.WriteEndObject();

        writer.WriteStartObject("paths");
        foreach (var (_, doc) in specs)
        {
            if (!doc.RootElement.TryGetProperty("paths", out var paths)) continue;
            foreach (var path in paths.EnumerateObject())
                path.WriteTo(writer);
        }
        writer.WriteEndObject();

        writer.WriteStartObject("components");
        writer.WriteStartObject("schemas");

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (tag, doc) in specs)
        {
            if (!doc.RootElement.TryGetProperty("components", out var components)) continue;
            if (!components.TryGetProperty("schemas", out var schemas)) continue;

            foreach (var schema in schemas.EnumerateObject())
            {
                var name = seen.Add(schema.Name)
                    ? schema.Name
                    : $"{char.ToUpperInvariant(tag[0])}{tag[1..]}_{schema.Name}";

                writer.WritePropertyName(name);
                schema.Value.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
        writer.WriteEndObject();
        writer.WriteEndObject();

        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
