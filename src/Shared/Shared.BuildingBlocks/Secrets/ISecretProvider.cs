namespace Shared.BuildingBlocks.Secrets;

/// <summary>
/// Abstraction for retrieving secrets from a secret store.
/// Implementations: Vault, Azure Key Vault, AWS Secrets Manager, environment variables.
/// </summary>
public interface ISecretProvider
{
    /// <summary>
    /// Retrieves a secret value by its path/key.
    /// </summary>
    /// <param name="path">The secret path (e.g., "eshopping/data/grafana" or "ConnectionStrings:OrderDb").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The secret value, or null if not found.</returns>
    Task<string?> GetSecretAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all key-value pairs under a secret path.
    /// Used by VaultConfigurationProvider to load multiple secrets at once.
    /// </summary>
    /// <param name="path">The mount path (e.g., "eshopping/data/database").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of secret key-value pairs.</returns>
    Task<IReadOnlyDictionary<string, string>> GetSecretsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the secret provider is available and healthy.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}
