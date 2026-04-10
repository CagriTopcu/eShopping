using Microsoft.Extensions.Configuration;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace ServiceDefaults.Secrets;

/// <summary>
/// IConfigurationProvider that loads secrets from HashiCorp Vault KV v2 engine.
/// Secrets are flattened into configuration keys using ":" separator.
/// Example: Vault path "eshopping/smtp" with key "Password" → "Smtp:Password".
/// </summary>
public sealed class VaultConfigurationProvider : ConfigurationProvider
{
    private readonly string _vaultUrl;
    private readonly string _token;
    private readonly string _mountPoint;
    private readonly string[] _secretPaths;

    public VaultConfigurationProvider(string vaultUrl, string token, string mountPoint, string[] secretPaths)
    {
        _vaultUrl = vaultUrl;
        _token = token;
        _mountPoint = mountPoint;
        _secretPaths = secretPaths;
    }

    public override void Load()
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var authMethod = new TokenAuthMethodInfo(_token);
            var settings = new VaultClientSettings(_vaultUrl, authMethod)
            {
                UseVaultTokenHeaderInsteadOfAuthorizationHeader = true
            };
            var client = new VaultClient(settings);

            foreach (var path in _secretPaths)
            {
                try
                {
                    var secret = client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                        path: path,
                        mountPoint: _mountPoint).GetAwaiter().GetResult();

                    if (secret?.Data?.Data is null)
                        continue;

                    // Convert Vault path to config section prefix:
                    // "eshopping/smtp" → "Smtp", "eshopping/grafana" → "Grafana"
                    var configPrefix = ConvertPathToConfigPrefix(path);

                    foreach (var kvp in secret.Data.Data)
                    {
                        if (kvp.Value is null) continue;

                        var configKey = string.IsNullOrEmpty(configPrefix)
                            ? kvp.Key
                            : $"{configPrefix}:{kvp.Key}";

                        data[configKey] = kvp.Value.ToString();
                    }
                }
                catch (Exception)
                {
                    // Individual path failure should not block other paths.
                    // In dev mode, Vault may not have all paths seeded yet.
                }
            }
        }
        catch (Exception)
        {
            // Vault unavailable at startup - fall back to other config sources.
            // This is expected in dev/test when Vault container isn't ready yet.
        }

        Data = data;
    }

    /// <summary>
    /// Converts Vault path to configuration prefix.
    /// "eshopping/smtp" → "Smtp"
    /// "eshopping/database/order" → "Database:Order"
    /// "connectionstrings" → "ConnectionStrings"
    /// </summary>
    private static string ConvertPathToConfigPrefix(string path)
    {
        // Strip common prefix like "eshopping/"
        var normalized = path;
        if (normalized.StartsWith("eshopping/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["eshopping/".Length..];
        }

        // Split remaining path and convert to PascalCase config sections
        var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(":", parts.Select(p =>
            char.ToUpperInvariant(p[0]) + p[1..]));
    }
}
