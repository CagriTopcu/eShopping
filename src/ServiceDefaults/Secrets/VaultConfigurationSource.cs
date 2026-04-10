using Microsoft.Extensions.Configuration;

namespace ServiceDefaults.Secrets;

/// <summary>
/// Configuration source that loads secrets from HashiCorp Vault into IConfiguration.
/// Only activates when VAULT_URL is present in environment.
/// </summary>
public sealed class VaultConfigurationSource : IConfigurationSource
{
    private readonly string _vaultUrl;
    private readonly string _token;
    private readonly string _mountPoint;
    private readonly string[] _secretPaths;

    /// <param name="vaultUrl">Vault server URL (e.g., http://localhost:8200).</param>
    /// <param name="token">Vault authentication token.</param>
    /// <param name="mountPoint">KV v2 mount point (default: "secret").</param>
    /// <param name="secretPaths">Vault secret paths to load (e.g., "eshopping/grafana", "eshopping/smtp").</param>
    public VaultConfigurationSource(string vaultUrl, string token, string mountPoint, string[] secretPaths)
    {
        _vaultUrl = vaultUrl;
        _token = token;
        _mountPoint = mountPoint;
        _secretPaths = secretPaths;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new VaultConfigurationProvider(_vaultUrl, _token, _mountPoint, _secretPaths);
}
