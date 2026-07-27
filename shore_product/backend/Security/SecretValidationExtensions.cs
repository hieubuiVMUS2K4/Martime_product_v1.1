using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProductApi.Security
{
    public static class SecretValidationExtensions
    {
        private static readonly string[] WeakSecrets = new[]
        {
            "CHANGE_ME",
            "your-api-key-here",
            "12345678901234567890123456789012",
            "Admin@123",
            "product_default_secret_key"
        };

        public static void ValidateProductionSecrets(this IConfiguration configuration, IHostEnvironment environment, ILogger logger)
        {
            if (!environment.IsProduction())
            {
                logger.LogInformation("Skipping strict secret key validation in non-production environment: {EnvironmentName}", environment.EnvironmentName);
                return;
            }

            logger.LogInformation("Validating production secrets & environment configuration...");

            var jwtKey = configuration["JWT:Key"] ?? configuration["JWT__Key"];
            ValidateSecret("JWT:Key", jwtKey, minLength: 32);

            var internalApiKey = configuration["InternalAccess:ApiKey"] ?? configuration["INTERNAL_API_KEY"];
            ValidateSecret("InternalAccess:ApiKey", internalApiKey, minLength: 32);

            var encryptionKey = configuration["DataProtection:EncryptionKey"] ?? configuration["DATA_PROTECTION__ENCRYPTION_KEY"];
            ValidateSecret("DataProtection:EncryptionKey", encryptionKey, minLength: 16);

            logger.LogInformation("All production secrets passed validation successfully.");
        }

        private static void ValidateSecret(string secretName, string? secretValue, int minLength)
        {
            if (string.IsNullOrWhiteSpace(secretValue))
            {
                throw new InvalidOperationException($"CRITICAL SECURITY ERROR: '{secretName}' is missing in production environment.");
            }

            if (secretValue.Length < minLength)
            {
                throw new InvalidOperationException($"CRITICAL SECURITY ERROR: '{secretName}' must be at least {minLength} characters long in production.");
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
