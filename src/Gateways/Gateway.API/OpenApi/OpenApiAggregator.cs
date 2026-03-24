using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Text;
using System.Text.Json;

namespace Gateway.API.OpenApi;

internal sealed class OpenApiAggregator
{
    public const string HttpClientName = "OpenApiAggregator";

    private static readonly FrozenDictionary<string, string> ServiceBaseAddresses =
        new Dictionary<string, string>
        {
            ["catalog"] = "http://catalog-api",
            ["basket"]  = "http://basket-api",
            ["orders"]  = "http://order-api"
        }.ToFrozenDictionary();

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenApiAggregator> _logger;
    private readonly ConcurrentDictionary<string, string> _cachedSpecs = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public OpenApiAggregator(
        IHttpClientFactory httpClientFactory,
        ILogger<OpenApiAggregator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> GetAggregatedSpecAsync(
        string version = "v1",
        bool refresh = false,
        CancellationToken ct = default)
    {
        if (!refresh && _cachedSpecs.TryGetValue(version, out var cached))
            return cached;

        await _lock.WaitAsync(ct);
        try
        {
            if (!refresh && _cachedSpecs.TryGetValue(version, out cached))
                return cached;

            var spec = await BuildMergedSpecAsync(version, ct);
            _cachedSpecs[version] = spec;
            return spec;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<string> BuildMergedSpecAsync(string version, CancellationToken ct)
    {
        var specs = new List<(string Tag, JsonDocument Doc)>();

        foreach (var (tag, baseAddress) in ServiceBaseAddresses)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientName);
                using var response = await client.GetAsync(
                    $"{baseAddress}/openapi/{version}.json", ct);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync(ct);
                var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                specs.Add((tag, doc));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to fetch OpenAPI spec from {Service} for version {Version}. Omitting from aggregated spec.",
                    tag, version);
            }
        }

        var result = MergeSpecs(specs, version);
        foreach (var (_, doc) in specs) doc.Dispose();
        return result;
    }

    private static string MergeSpecs(List<(string Tag, JsonDocument Doc)> specs, string version)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

        writer.WriteStartObject();
        writer.WriteString("openapi", "3.0.1");

        writer.WriteStartObject("info");
        writer.WriteString("title", "eShopping API");
        writer.WriteString("version", version);
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
