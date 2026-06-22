using ErrorOr;

namespace Nexova.Queries.Errors;

public static class QueryErrors
{
    public static Error SqlRequired => Error.Validation(
        code: "Query.SqlRequired",
        description: "A SQL statement is required");

    public static Error DataSourceRequired => Error.Validation(
        code: "Query.DataSourceRequired",
        description: "At least one data source is required");
}
