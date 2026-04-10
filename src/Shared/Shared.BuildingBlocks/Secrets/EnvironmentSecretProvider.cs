using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Shared.BuildingBlocks.Secrets;

/// <summary>
/// Reads secrets from environment variables and IConfiguration.
/// Used in development/Aspire where Vault is not available.
/// Falls back to: Environment Variable → IConfiguration → null.
/// </summary>
public sealed class EnvironmentSecretProvider : ISecretProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EnvironmentSecretProvider> _logger;

    public EnvironmentSecretProvider(IConfiguration configuration, ILogger<EnvironmentSecretProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<string?> GetSecretAsync(string path, CancellationToken cancellationToken = default)
    {
        // Try environment variable first (path with : replaced by __ for env var convention)
        var envKey = path.Replace(":", "__").Replace("/", "__");
        var value = Environment.GetEnvironmentVariable(envKey);

        if (value is not null)
        {
            _logger.LogDebug("Secret '{Path}' resolved from environment variable", path);
            return Task.FromResult<string?>(value);
        }

        // Fall back to IConfiguration (supports appsettings, user-secrets, etc.)
        value = _configuration[path];

        if (value is not null)
        {
            _logger.LogDebug("Secret '{Path}' resolved from configuration", path);
        }
        else
        {
            _logger.LogDebug("Secret '{Path}' not found in environment or configuration", path);
        }

        return Task.FromResult<string?>(value);
    }

    public Task<IReadOnlyDictionary<string, string>> GetSecretsAsync(
        string path, CancellationToken cancellationToken = default)
    {
        var section = _configuration.GetSection(path.Replace("/", ":"));
        var secrets = new Dictionary<string, string>();

        if (section.Exists())
        {
            foreach (var child in section.GetChildren())
            {
                if (child.Value is not null)
                {
                    secrets[child.Key] = child.Value;
                }
            }
        }

        return Task.FromResult<IReadOnlyDictionary<string, string>>(secrets);
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(true);
}
