using System.Diagnostics;

namespace Shared.BuildingBlocks.Observability;

/// <summary>
/// Shared ActivitySource for custom spans across all eShopping services.
/// Register via OpenTelemetry: .AddSource(DiagnosticConfig.SourceName)
/// </summary>
public static class DiagnosticConfig
{
    public const string SourceName = "eShopping";

    public static readonly ActivitySource Source = new(SourceName, "1.0.0");
}
