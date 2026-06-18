using System.Diagnostics;
using System.Text;
using Apache.Arrow;
using Apache.Arrow.Types;
using Apache.DataFusion;
using Nexova.Core.Entities;
using Nexova.Connectors.Abstractions;

namespace Nexova.Connectors;

public sealed class DataFusionQueryExecutor(RegisteredSessionProvider sessionProvider) : IQueryExecutor
{
    private const string ListTablesSql =
        """
        SELECT table_schema, table_name, table_type
        FROM information_schema.tables
        WHERE table_schema <> 'information_schema'
        ORDER BY table_name
        """;

    private const string FirstTableSql =
        """
        SELECT table_schema, table_name
        FROM information_schema.tables
        WHERE table_schema <> 'information_schema'
        LIMIT 1
        """;

    public async Task<QueryResult> ExecuteAsync(
        string sql,
        IReadOnlyCollection<DataSource> dataSources,
        int? maxRows = null,
        CancellationToken cancellationToken = default)
    {
        var context = await sessionProvider.GetAsync(dataSources, cancellationToken);

        using var dataFrame = context.Sql(sql);
        return await ReadResultAsync(dataFrame, maxRows, cancellationToken);
    }

    public async Task<IReadOnlyList<TableInfo>> ListTablesAsync(
        DataSource dataSource,
        CancellationToken cancellationToken = default)
    {
        var context = await sessionProvider.GetAsync([dataSource], cancellationToken);

        using var dataFrame = context.Sql(ListTablesSql);
        var result = await ReadResultAsync(dataFrame, null, cancellationToken);

        return result.Rows
            .Select(row => new TableInfo(
                row[0]?.ToString(),
                row[1]?.ToString() ?? string.Empty,
                row[2]?.ToString() ?? string.Empty))
            .ToList();
    }

    public async Task<IReadOnlyList<ColumnInfo>> ListColumnsAsync(
        DataSource dataSource,
        string table,
        string? schema = null,
        CancellationToken cancellationToken = default)
    {
        var context = await sessionProvider.GetAsync([dataSource], cancellationToken);

        using var dataFrame = context.Sql($"SELECT * FROM {QuoteTableReference(schema, table)} LIMIT 0");
        return MapColumns(dataFrame.Schema());
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(
        DataSource dataSource,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var context = await sessionProvider.GetAsync([dataSource], cancellationToken);

            using var tablesFrame = context.Sql(FirstTableSql);
            var tables = await ReadResultAsync(tablesFrame, 1, cancellationToken);

            if (tables.Rows.Count > 0 && tables.Rows[0][0] is string schema && tables.Rows[0][1] is string name)
            {
                using var probe = context.Sql($"SELECT * FROM {QuoteTableReference(schema, name)} LIMIT 1");
                using var reader = probe.ExecuteStream(cancellationToken);
                _ = await reader.ReadNextRecordBatchAsync(cancellationToken);
            }

            stopwatch.Stop();
            return new ConnectionTestResult(true, null, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return new ConnectionTestResult(false, exception.Message, stopwatch.ElapsedMilliseconds);
        }
    }

    private static async Task<QueryResult> ReadResultAsync(
        DataFrame dataFrame,
        int? maxRows,
        CancellationToken cancellationToken)
    {
        var columns = MapColumns(dataFrame.Schema());

        var rows = new List<IReadOnlyList<object?>>();
        using var reader = dataFrame.ExecuteStream(cancellationToken);

        while (await reader.ReadNextRecordBatchAsync(cancellationToken) is { } batch)
        {
            using (batch)
            {
                var columnArrays = batch.Arrays.ToArray();
                for (var rowIndex = 0; rowIndex < batch.Length; rowIndex++)
                {
                    if (maxRows is { } limit && rows.Count >= limit)
                    {
                        return new QueryResult { Columns = columns, Rows = rows };
                    }

                    rows.Add(ReadRow(columnArrays, rowIndex));
                }
            }
        }

        return new QueryResult { Columns = columns, Rows = rows };
    }

    private static string QuoteIdentifier(string identifier)
    {
        const string quote = "\"";
        return quote + identifier.Replace(quote, quote + quote) + quote;
    }

    private static string QuoteTableReference(string? schema, string table) =>
        string.IsNullOrWhiteSpace(schema)
            ? QuoteIdentifier(table)
            : $"{QuoteIdentifier(schema)}.{QuoteIdentifier(table)}";

    private static IReadOnlyList<ColumnInfo> MapColumns(Schema schema) =>
        schema.FieldsList.Select(MapColumn).ToList();

    private static ColumnInfo MapColumn(Field field)
    {
        var (precision, scale) = field.DataType switch
        {
            Decimal128Type type => ((int?)type.Precision, (int?)type.Scale),
            Decimal256Type type => ((int?)type.Precision, (int?)type.Scale),
            _ => (null, null)
        };

        return new ColumnInfo(field.Name, field.DataType.Name, field.IsNullable, precision, scale);
    }

    private static object?[] ReadRow(IReadOnlyList<IArrowArray> columnArrays, int rowIndex)
    {
        var row = new object?[columnArrays.Count];
        for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
        {
            row[columnIndex] = ReadValue(columnArrays[columnIndex], rowIndex);
        }

        return row;
    }

    private static object? ReadValue(IArrowArray array, int index)
    {
        if (array.IsNull(index))
        {
            return null;
        }

        return array switch
        {
            BooleanArray value => value.GetValue(index),
            Int8Array value => value.GetValue(index),
            Int16Array value => value.GetValue(index),
            Int32Array value => value.GetValue(index),
            Int64Array value => value.GetValue(index),
            UInt8Array value => value.GetValue(index),
            UInt16Array value => value.GetValue(index),
            UInt32Array value => value.GetValue(index),
            UInt64Array value => value.GetValue(index),
            HalfFloatArray value => value.GetValue(index) is { } half ? (double)half : null,
            FloatArray value => value.GetValue(index),
            DoubleArray value => value.GetValue(index),
            Decimal128Array value => value.GetValue(index),
            Decimal256Array value => value.GetString(index),
            Date32Array value => value.GetDateTimeOffset(index),
            Date64Array value => value.GetDateTimeOffset(index),
            TimestampArray value => value.GetTimestamp(index),
            Time32Array value => value.GetValue(index),
            Time64Array value => value.GetValue(index),
            StringArray value => value.GetString(index, Encoding.UTF8),
            LargeStringArray value => value.GetString(index, Encoding.UTF8),
            BinaryArray value => value.GetBytes(index).ToArray(),
            LargeBinaryArray value => value.GetBytes(index).ToArray(),
            _ => null
        };
    }
}
