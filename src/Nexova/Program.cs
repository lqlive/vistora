using FluentValidation;
using Nexova.Charts;
using Nexova.Charts.Http;
using Nexova.Connectors;
using Nexova.Core.Management;
using Nexova.Database.PostgreSql;
using Nexova.Datasets;
using Nexova.Datasets.Http;
using Nexova.DataSources;
using Nexova.DataSources.Http;
using Nexova.Queries;
using Nexova.Queries.Http;
using Nexova.Storage.Aws;
using Nexova.Users;
using Nexova.Users.Authentication;
using Nexova.Users.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNexovaCore()
    .AddInMemoryStore()
    .AddPostgreSqlDatabase()
    .AddFileStorage()
    .AddAwsS3Storage()
    .AddDataFusionConnectors();

builder.Services.AddScoped<DataSourceService>();
builder.Services.AddScoped<DatasetService>();
builder.Services.AddScoped<ChartService>();
builder.Services.AddScoped<QueryService>();
builder.Services.AddScoped<QueryDocumentService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddGitHubAuthentication(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapDataSourceApi().RequireAuthorization();
app.MapDatasetApi().RequireAuthorization();
app.MapChartApi().RequireAuthorization();
app.MapQueryApi().RequireAuthorization();
app.MapQueryDocumentApi().RequireAuthorization();
app.MapAuthApi();

app.Run();