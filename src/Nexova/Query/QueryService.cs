using System.Diagnostics;
using ErrorOr;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;
using Nexova.Core.Stores;
using Nexova.DataSources.Errors;
using Nexova.Query.Errors;
using Nexova.Query.Models;

namespace Nexova.Query;

public sealed class QueryService(IDataSourceStore dataSourceStore, IQueryExecutor queryExecutor)
{
    public async Task<ErrorOr<QueryResponse>> ExecuteAsync(
        QueryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Sql))
        {
            return QueryErrors.SqlRequired;
        }

        if (request.DataSourceIds is null || request.DataSourceIds.Count == 0)
        {
            return QueryErrors.DataSourceRequired;
        }

        var resolved = await ResolveAsync(request.DataSourceIds, cancellationToken);
        if (resolved.IsError)
        {
            return resolved.Errors;
        }

        var stopwatch = Stopwatch.StartNew();
        var result = await queryExecutor.ExecuteAsync(
            request.Sql,
            resolved.Value,
            maxRows: request.Limit,
            cancellationToken: cancellationToken);
        stopwatch.Stop();

        return new QueryResponse(result.Columns, result.Rows, result.Rows.Count, stopwatch.ElapsedMilliseconds);
    }

    public async Task<ErrorOr<ExplainResponse>> ExplainAsync(
        ExplainRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Sql))
        {
            return QueryErrors.SqlRequired;
        }

        var dataSource = await dataSourceStore.GetAsync(request.DataSourceId, cancellationToken);
        if (dataSource is null)
        {
            return DataSourceErrors.NotFound;
        }

        var stopwatch = Stopwatch.StartNew();
        var result = await queryExecutor.ExecuteAsync(
            $"EXPLAIN {request.Sql}",
            [dataSource],
            cancellationToken: cancellationToken);
        stopwatch.Stop();

        var plans = result.Rows
            .Where(row => row.Count >= 2)
            .Select(row => new ExplainPlanInfo(
                row[0]?.ToString() ?? string.Empty,
                row[1]?.ToString() ?? string.Empty))
            .ToList();

        var logicalPlan = plans.FirstOrDefault(plan => plan.PlanType == "logical_plan")?.Plan;
        var physicalPlan = plans.FirstOrDefault(plan => plan.PlanType == "physical_plan")?.Plan;

        return new ExplainResponse(logicalPlan, physicalPlan, plans, stopwatch.ElapsedMilliseconds);
    }

    private async Task<ErrorOr<IReadOnlyCollection<DataSource>>> ResolveAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken)
    {
        var dataSources = new List<DataSource>(ids.Count);
        foreach (var id in ids)
        {
            var dataSource = await dataSourceStore.GetAsync(id, cancellationToken);
            if (dataSource is null)
            {
                return DataSourceErrors.NotFound;
            }

            dataSources.Add(dataSource);
        }

        return dataSources;
    }
}
