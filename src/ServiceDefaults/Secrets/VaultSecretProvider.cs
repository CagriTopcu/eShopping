using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.Secrets;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace ServiceDefaults.Secrets;

/// <summary>
/// HashiCorp Vault secret provider using VaultSharp.
/// Reads from KV v2 secret engine at the configured mount point.
/// </summary>
public sealed class VaultSecretProvider : ISecretProvider
{
    private readonly IVaultClient _client;
    private readonly ILogger<VaultSecretProvider> _logger;
    private readonly string _mountPoint;

    public VaultSecretProvider(string vaultUrl, string token, string mountPoint, ILogger<VaultSecretProvider> logger)
    {
        _mountPoint = mountPoint;
        _logger = logger;

        var authMethod = new TokenAuthMethodInfo(token);
        var settings = new VaultClientSettings(vaultUrl, authMethod)
        {
            UseVaultTokenHeaderInsteadOfAuthorizationHeader = true
        };
        _client = new VaultClient(settings);
    }

    public async Task<string?> GetSecretAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            // Path format: "section/key" → Vault path "section", data key "key"
            var (vaultPath, key) = ParsePath(path);

            var secret = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                path: vaultPath,
                mountPoint: _mountPoint);

            if (secret?.Data?.Data is not null && secret.Data.Data.TryGetValue(key, out var value))
            {
                _logger.LogDebug("Secret '{Path}' resolved from Vault", path);
                return value?.ToString();
            }

            _logger.LogDebug("Secret '{Path}' not found in Vault at mount '{Mount}'", path, _mountPoint);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read secret '{Path}' from Vault", path);
            return null;
        }
    }

    public async Task<IReadOnlyDictionary<string, string>> GetSecretsAsync(
        string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var secret = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                path: path,
                mountPoint: _mountPoint);

            if (secret?.Data?.Data is null)
            {
                return new Dictionary<string, string>();
            }

            return secret.Data.Data
                .Where(kvp => kvp.Value is not null)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.ToString()!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read secrets at path '{Path}' from Vault", path);
            return new Dictionary<string, string>();
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await _client.V1.System.GetHealthStatusAsync();
            return health.Initialized && !health.Sealed;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Vault health check failed");
            return false;
        }
    }

    private static (string vaultPath, string key) ParsePath(string path)
    {
        // "database/connectionstring" → vaultPath="database", key="connectionstring"
        // "ConnectionStrings:OrderDb" → vaultPath="ConnectionStrings", key="OrderDb"
        var normalized = path.Replace(":", "/");
        var lastSlash = normalized.LastIndexOf('/');

        if (lastSlash < 0)
        {
            return (normalized, "value");
        }

        return (normalized[..lastSlash], normalized[(lastSlash + 1)..]);
    }
}
