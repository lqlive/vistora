using ErrorOr;

namespace Nexova.Charts.Errors;

public static class ChartErrors
{
    public static Error NotFound => Error.NotFound(
        code: "Chart.NotFound",
        description: "Chart not found");

    public static Error NameRequired => Error.Validation(
        code: "Chart.NameRequired",
        description: "Chart name is required");

    public static Error NameTooLong => Error.Validation(
        code: "Chart.NameTooLong",
        description: "Chart name must not exceed 256 characters");

    public static Error VizTypeRequired => Error.Validation(
        code: "Chart.VizTypeRequired",
        description: "Chart visualization type is required");

    public static Error VizTypeTooLong => Error.Validation(
        code: "Chart.VizTypeTooLong",
        description: "Chart visualization type must not exceed 64 characters");

    public static Error DatasetRequired => Error.Validation(
        code: "Chart.DatasetRequired",
        description: "Chart dataset is required");

    public static Error DatasetTooLong => Error.Validation(
        code: "Chart.DatasetTooLong",
        description: "Chart dataset must not exceed 256 characters");

    public static Error NameAlreadyExists => Error.Conflict(
        code: "Chart.NameAlreadyExists",
        description: "A chart with the same name already exists");
}
