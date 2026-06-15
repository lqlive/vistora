using System.Text;
using Apache.DataFusion;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;
using Nexova.Core.Storage;

namespace Nexova.Connectors;

public sealed class FileConnector(IStorageService storageService) : IConnector
{
    public DataSourceType Type => DataSourceType.Files;

    public async Task RegisterAsync(SessionContext context, string tableName,
        DataSource dataSource, CancellationToken cancellationToken)
    {
        foreach (var asset in dataSource.FileAssets)
        {
            var uri = await storageService.GetDownloadUriAsync(asset.StoragePath, cancellationToken);
            var path = uri.IsFile ? uri.LocalPath : uri.ToString();

            Register(context, ResolveTableName(asset), path, asset);
        }
    }

    private static void Register(SessionContext context, string tableName, string path, DataSourceFileAsset asset)
    {
        switch (ResolveFormat(asset))
        {
            case FileFormat.Csv:
                context.RegisterCsv(tableName, path, new CsvReadOptions
                {
                    HasHeader = asset.HasHeader ?? true,
                    Delimiter = ResolveDelimiter(asset)
                });
                break;
            case FileFormat.Parquet:
                context.RegisterParquet(tableName, path);
                break;
            case FileFormat.Json:
                context.RegisterJson(tableName, path);
                break;
            case FileFormat.Arrow:
                context.RegisterArrow(tableName, path);
                break;
            case FileFormat.Avro:
                context.RegisterAvro(tableName, path);
                break;
            default:
                throw new NotSupportedException(
                    $"Unsupported file format for '{asset.FileName}' (content type '{asset.ContentType}').");
        }
    }

    private static FileFormat ResolveFormat(DataSourceFileAsset asset)
    {
        var extension = Path.GetExtension(asset.FileName).ToLowerInvariant();
        return extension switch
        {
            ".csv" or ".tsv" or ".txt" => FileFormat.Csv,
            ".parquet" or ".pqt" => FileFormat.Parquet,
            ".json" or ".ndjson" => FileFormat.Json,
            ".arrow" => FileFormat.Arrow,
            ".avro" => FileFormat.Avro,
            _ => ResolveFromContentType(asset.ContentType)
        };
    }

    private static FileFormat ResolveFromContentType(string contentType) => contentType.ToLowerInvariant() switch
    {
        "text/csv" or "application/csv" => FileFormat.Csv,
        "application/json" or "application/x-ndjson" or "application/ndjson" => FileFormat.Json,
        "application/vnd.apache.parquet" or "application/parquet" => FileFormat.Parquet,
        _ => FileFormat.Unknown
    };

    private static byte ResolveDelimiter(DataSourceFileAsset asset)
    {
        if (!string.IsNullOrEmpty(asset.Delimiter))
        {
            return (byte)asset.Delimiter[0];
        }

        return Path.GetExtension(asset.FileName).Equals(".tsv", StringComparison.OrdinalIgnoreCase)
            ? (byte)'\t'
            : (byte)',';
    }

    private static string ResolveTableName(DataSourceFileAsset asset)
    {
        var baseName = Path.GetFileNameWithoutExtension(asset.FileName);
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = asset.Id.ToString("N");
        }

        var builder = new StringBuilder(baseName.Length);
        foreach (var character in baseName)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        }

        var name = builder.ToString();
        return char.IsDigit(name[0]) ? "_" + name : name;
    }

    private enum FileFormat
    {
        Unknown,
        Csv,
        Parquet,
        Json,
        Arrow,
        Avro
    }
}
