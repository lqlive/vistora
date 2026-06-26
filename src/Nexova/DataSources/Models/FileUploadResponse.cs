namespace Nexova.DataSources.Models;

public sealed record FileUploadResponse(
    string FileName,
    string ContentType,
    long Size,
    string StoragePath,
    string Path);
