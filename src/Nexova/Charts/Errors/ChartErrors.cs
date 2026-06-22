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

    public static Error VizTypeRequired => Error.Validation(
        code: "Chart.VizTypeRequired",
        description: "Chart visualization type is required");

    public static Error DatasetRequired => Error.Validation(
        code: "Chart.DatasetRequired",
        description: "Chart dataset is required");

    public static Error NameAlreadyExists => Error.Conflict(
        code: "Chart.NameAlreadyExists",
        description: "A chart with the same name already exists");
}
