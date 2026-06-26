using ErrorOr;

namespace Nexova.Queries.Errors;

public static class QueryDocumentErrors
{
    public static Error NotFound => Error.NotFound(
        code: "QueryDocument.NotFound",
        description: "Query document not found");

    public static Error NameRequired => Error.Validation(
        code: "QueryDocument.NameRequired",
        description: "Query document name is required");

    public static Error SqlRequired => Error.Validation(
        code: "QueryDocument.SqlRequired",
        description: "A SQL statement is required");

    public static Error NameAlreadyExists => Error.Conflict(
        code: "QueryDocument.NameAlreadyExists",
        description: "A query document with the same name already exists");

    public static Error AccessDenied => Error.Unauthorized(
        code: "QueryDocument.AccessDenied",
        description: "You do not have access to this query document");
}
