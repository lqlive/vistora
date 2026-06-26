using Microsoft.Extensions.Options;

namespace Nexova.Core.Configuration.Validators;

public class StorageOptionsValidator : IValidateOptions<StorageOptions>
{
    public ValidateOptionsResult Validate(string? name, StorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Type))
        {
            return ValidateOptionsResult.Fail("Storage:Type cannot be blank.");
        }

        return ValidateOptionsResult.Success;
    }
}
