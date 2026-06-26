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

    public async Task<IReadOnlyList<ColumnInfo>> DescribeAsync(
        string sql,
        IReadOnlyCollection<DataSource> dataSources,
        CancellationToken cancellationToken = default)
    {
        var context = await sessionProvider.GetAsync(dataSources, cancellationToken);

        var inner = sql.TrimEnd().TrimEnd(';');
        using var dataFrame = context.Sql($"SELECT * FROM ({inner}) AS nexova_schema LIMIT 0");
        return MapColumns(dataFrame.Schema());
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

        var rows = new List<IReadOnlyList<object?>>(maxRows.GetValueOrDefault());
        using var reader = dataFrame.ExecuteStream(cancellationToken);

        while (await reader.ReadNextRecordBatchAsync(cancellationToken) is { } batch)
        {
            using (batch)
            {
                var columnArrays = batch.Arrays.ToArray();
                var valueReaders = columnArrays.Select(CreateValueReader).ToArray();
                var rowCount = maxRows is { } limit
                    ? Math.Min(batch.Length, limit - rows.Count)
                    : batch.Length;

                for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    rows.Add(ReadRow(valueReaders, rowIndex));
                }

                if (maxRows is { } rowLimit && rows.Count >= rowLimit)
                {
                    return new QueryResult { Columns = columns, Rows = rows };
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

    private static object?[] ReadRow(IReadOnlyList<Func<int, object?>> valueReaders, int rowIndex)
    {
        var row = new object?[valueReaders.Count];
        for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
        {
            row[columnIndex] = valueReaders[columnIndex](rowIndex);
        }

        return row;
    }

    private static Func<int, object?> CreateValueReader(IArrowArray array) =>
        array switch
        {
            BooleanArray value => index => ReadValue(value, index, value.GetValue),
            Int8Array value => index => ReadValue(value, index, value.GetValue),
            Int16Array value => index => ReadValue(value, index, value.GetValue),
            Int32Array value => index => ReadValue(value, index, value.GetValue),
            Int64Array value => index => ReadValue(value, index, value.GetValue),
            UInt8Array value => index => ReadValue(value, index, value.GetValue),
            UInt16Array value => index => ReadValue(value, index, value.GetValue),
            UInt32Array value => index => ReadValue(value, index, value.GetValue),
            UInt64Array value => index => ReadValue(value, index, value.GetValue),
            HalfFloatArray value => index => value.IsNull(index) || value.GetValue(index) is not { } half ? null : (double)half,
            FloatArray value => index => ReadValue(value, index, value.GetValue),
            DoubleArray value => index => ReadValue(value, index, value.GetValue),
            Decimal128Array value => index => ReadValue(value, index, value.GetValue),
            Decimal256Array value => index => ReadValue(value, index, value.GetString),
            Date32Array value => index => ReadValue(value, index, value.GetDateTimeOffset),
            Date64Array value => index => ReadValue(value, index, value.GetDateTimeOffset),
            TimestampArray value => index => ReadValue(value, index, value.GetTimestamp),
            Time32Array value => index => ReadValue(value, index, value.GetValue),
            Time64Array value => index => ReadValue(value, index, value.GetValue),
            StringArray value => index => value.IsNull(index) ? null : value.GetString(index, Encoding.UTF8),
            LargeStringArray value => index => value.IsNull(index) ? null : value.GetString(index, Encoding.UTF8),
            BinaryArray value => index => value.IsNull(index) ? null : value.GetBytes(index).ToArray(),
            LargeBinaryArray value => index => value.IsNull(index) ? null : value.GetBytes(index).ToArray(),
            _ => _ => null
        };

    private static object? ReadValue<TValue>(
        IArrowArray array,
        int index,
        Func<int, TValue> read) =>
        array.IsNull(index) ? null : read(index);
}
