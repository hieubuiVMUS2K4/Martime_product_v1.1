using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MaritimeEdge.Security
{
    public static class SecretValidationExtensions
    {
        private static readonly string[] WeakSecrets = new[]
        {
            "CHANGE_ME",
            "your-api-key-here",
            "12345678901234567890123456789012",
            "Admin@123",
            "edge_default_secret_key"
        };

        public static void ValidateProductionSecrets(this IConfiguration configuration, IHostEnvironment environment, ILogger logger)
        {
            if (!environment.IsProduction())
            {
                logger.LogInformation("Skipping strict secret key validation in non-production environment: {EnvironmentName}", environment.EnvironmentName);
                return;
            }

            logger.LogInformation("Validating edge production secrets & environment configuration...");

            var tokenKey = configuration["Auth:TokenSigningKey"] ?? Environment.GetEnvironmentVariable("EDGE_AUTH_TOKEN_SIGNING_KEY");
            ValidateSecret("Auth:TokenSigningKey", tokenKey, minLength: 32);

            var internalApiKey = configuration["InternalAccess:ApiKey"] ?? Environment.GetEnvironmentVariable("EDGE_INTERNAL_API_KEY");
            ValidateSecret("InternalAccess:ApiKey", internalApiKey, minLength: 32);

            logger.LogInformation("All edge production secrets passed validation successfully.");
        }

        private static void ValidateSecret(string secretName, string? secretValue, int minLength)
        {
            if (string.IsNullOrWhiteSpace(secretValue))
            {
                throw new InvalidOperationException($"CRITICAL SECURITY ERROR: '{secretName}' is missing in edge production environment.");
            }

            if (secretValue.Length < minLength)
            {
                throw new InvalidOperationException($"CRITICAL SECURITY ERROR: '{secretName}' must be at least {minLength} characters long in edge production.");
            }

            foreach (var weak in WeakSecrets)
            {
                if (secretValue.Contains(weak, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"CRITICAL SECURITY ERROR: '{secretName}' contains insecure default placeholder string '{weak}'. Please configure a strong random secret.");
                }
            }
        }
    }
}
