using ErrorOr;

namespace Nexova.Datasets.Errors;

public static class DatasetErrors
{
    public static Error NotFound => Error.NotFound(
        code: "Dataset.NotFound",
        description: "Dataset not found");

    public static Error NameRequired => Error.Validation(
        code: "Dataset.NameRequired",
        description: "Dataset name is required");

    public static Error SqlRequired => Error.Validation(
        code: "Dataset.SqlRequired",
        description: "A SQL statement is required");

    public static Error NameAlreadyExists => Error.Conflict(
        code: "Dataset.NameAlreadyExists",
        description: "A dataset with the same name already exists");
}
