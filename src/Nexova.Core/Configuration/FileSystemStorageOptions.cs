namespace Nexova.Core.Configuration;

public class FileSystemStorageOptions
{
    public const string Name = "FileSystem";
    public const string SectionName = "FileSystemStorage";
    public required string Path { get; set; }
}