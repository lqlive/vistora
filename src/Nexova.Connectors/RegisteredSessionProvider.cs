using System.Collections.Concurrent;
using Apache.DataFusion;
using Microsoft.Extensions.DependencyInjection;
using Nexova.Connectors.Abstractions;
using Nexova.Core.Entities;

namespace Nexova.Connectors;

public sealed class RegisteredSessionProvider(IServiceScopeFactory scopeFactory) : IDisposable
{
    private readonly ConcurrentDictionary<string, Lazy<Task<RegisteredSession>>> sessions = new();

    public async Task<SessionContext> GetAsync(
        IReadOnlyCollection<DataSource> dataSources,
        CancellationToken cancellationToken)
    {
        var sources = dataSources
            .OrderBy(static dataSource => dataSource.Id)
            .ToArray();
        var key = CreateSessionKey(sources);
        var session = sessions.GetOrAdd(
            key,
            _ => new Lazy<Task<RegisteredSession>>(
                () => CreateAsync(sources, cancellationToken),
                LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            return (await session.Value).Context;
        }
        catch
        {
            sessions.TryRemove(KeyValuePair.Create(key, session));
            throw;
        }
    }

    public void Dispose()
    {
        foreach (var session in sessions.Values)
        {
            if (session.IsValueCreated && session.Value.IsCompletedSuccessfully)
            {
                session.Value.Result.Context.Dispose();
            }
        }

        sessions.Clear();
    }

    private async Task<RegisteredSession> CreateAsync(
        IReadOnlyCollection<DataSource> dataSources,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = SessionContext.CreateBuilder().InformationSchema(true).Build();

        try
        {
            foreach (var dataSource in dataSources)
            {
                var connector = scope.ServiceProvider.GetRequiredKeyedService<IConnector>(dataSource.Type);
                await connector.RegisterAsync(context, dataSource.Name, dataSource, cancellationToken);
            }

            return new RegisteredSession(context);
        }
        catch
        {
            context.Dispose();
            throw;
        }
    }

    private static string CreateSessionKey(IReadOnlyCollection<DataSource> dataSources) =>
        string.Join('|', dataSources.Select(CreateSessionKeyPart));

    private static string CreateSessionKeyPart(DataSource dataSource) =>
        string.Join(':',
            dataSource.Id.ToString("N"),
            dataSource.Type,
            dataSource.UpdatedAt.UtcTicks);

    private sealed record RegisteredSession(SessionContext Context);
}
