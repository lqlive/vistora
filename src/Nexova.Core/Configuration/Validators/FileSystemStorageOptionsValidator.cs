using Microsoft.Extensions.Options;

namespace Nexova.Core.Configuration.Validators;

public class FileSystemStorageOptionsValidator : IValidateOptions<FileSystemStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, FileSystemStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Path))
        {
            return ValidateOptionsResult.Fail("FileSystemStorage:Path cannot be blank.");
        }

        return ValidateOptionsResult.Success;
    }
}
