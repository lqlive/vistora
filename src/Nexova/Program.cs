using FluentValidation;
using Nexova.Connectors;
using Nexova.Core.Management;
using Nexova.Database.PostgreSql;
using Nexova.DataSources;
using Nexova.DataSources.Http;
using Nexova.Query;
using Nexova.Query.Http;
using Nexova.Storage.Aws;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNexovaCore()
    .AddInMemoryStore()
    .AddPostgreSqlDatabase()
    .AddFileStorage()
    .AddAwsS3Storage()
    .AddDataFusionConnectors();

builder.Services.AddScoped<DataSourceService>();
builder.Services.AddScoped<QueryService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.MapDataSourceApi();
app.MapQueryApi();

app.Run();