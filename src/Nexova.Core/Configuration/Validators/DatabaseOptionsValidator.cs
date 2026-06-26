using Microsoft.Extensions.Options;

namespace Nexova.Core.Configuration.Validators;

public class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Type))
        {
            return ValidateOptionsResult.Fail("Database:Type cannot be blank.");
        }

        // Relational providers need a connection string; the in-memory store does not.
        var requiresConnectionString = !string.Equals(
            options.Type, InMemoryStoreOptions.Name, StringComparison.OrdinalIgnoreCase);
        if (requiresConnectionString && string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail(
                $"Database:ConnectionString is required when Database:Type is '{options.Type}'.");
        }

        return ValidateOptionsResult.Success;
    }
}
