using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.Secrets;

namespace ServiceDefaults.Secrets;

public static class SecretProviderExtensions
{
    /// <summary>
    /// Default Vault secret paths loaded into IConfiguration.
    /// Each path maps to a config section (e.g., "eshopping/smtp" → "Smtp:*").
    /// </summary>
    private static readonly string[] DefaultSecretPaths =
    [
        "eshopping/grafana",
        "eshopping/smtp",
        "eshopping/database",
        "eshopping/rabbitmq",
        "eshopping/keycloak"
    ];

    /// <summary>
    /// Default rotation TTL for cached secrets.
    /// Secrets are refreshed from Vault after this period.
    /// </summary>
    private static readonly TimeSpan DefaultRotationTtl = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Registers the appropriate ISecretProvider based on environment.
    /// If VAULT_URL is set → VaultSecretProvider (with rotation cache), otherwise → EnvironmentSecretProvider.
    /// </summary>
    public static IServiceCollection AddSecretProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var vaultUrl = configuration["VAULT_URL"] ?? configuration["Vault:Url"];
        var vaultToken = configuration["VAULT_TOKEN"] ?? configuration["Vault:Token"];
        var mountPoint = configuration["VAULT_MOUNT_POINT"] ?? configuration["Vault:MountPoint"] ?? "secret";

        if (!string.IsNullOrEmpty(vaultUrl) && !string.IsNullOrEmpty(vaultToken))
        {
            services.AddSingleton<ISecretProvider>(sp =>
            {
                var vaultLogger = sp.GetRequiredService<ILogger<VaultSecretProvider>>();
                var rotatingLogger = sp.GetRequiredService<ILogger<RotatingSecretProvider>>();

                ISecretProvider vault = new VaultSecretProvider(vaultUrl, vaultToken, mountPoint, vaultLogger);

                // Wrap with rotation-aware caching decorator
                return new RotatingSecretProvider(vault, DefaultRotationTtl, rotatingLogger);
            });
        }
        else
        {
            services.AddSingleton<ISecretProvider, EnvironmentSecretProvider>();
        }

        return services;
    }

    /// <summary>
    /// Adds Vault as a configuration source if VAULT_URL is set.
    /// Loads secrets from Vault KV v2 into IConfiguration at startup.
    /// Falls back gracefully if Vault is unavailable (dev/test scenarios).
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <param name="secretPaths">Optional custom secret paths. If null, uses default paths.</param>
    public static IConfigurationBuilder AddVaultConfiguration(
        this IConfigurationBuilder builder,
        string[]? secretPaths = null)
    {
        // Build interim config to read VAULT_URL from env/appsettings
        var interimConfig = builder.Build();

        var vaultUrl = interimConfig["VAULT_URL"] ?? interimConfig["Vault:Url"];
        var vaultToken = interimConfig["VAULT_TOKEN"] ?? interimConfig["Vault:Token"];
        var mountPoint = interimConfig["VAULT_MOUNT_POINT"] ?? interimConfig["Vault:MountPoint"] ?? "secret";

        if (string.IsNullOrEmpty(vaultUrl) || string.IsNullOrEmpty(vaultToken))
        {
            return builder;
        }

        builder.Add(new VaultConfigurationSource(
            vaultUrl,
            vaultToken,
            mountPoint,
            secretPaths ?? DefaultSecretPaths));

        return builder;
    }
}
