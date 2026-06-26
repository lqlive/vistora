using ErrorOr;

namespace Nexova.Dashboards.Errors;

public static class DashboardErrors
{
    public static Error NotFound => Error.NotFound(
        code: "Dashboard.NotFound",
        description: "Dashboard not found");

    public static Error NameRequired => Error.Validation(
        code: "Dashboard.NameRequired",
        description: "Dashboard name is required");

    public static Error NameTooLong => Error.Validation(
        code: "Dashboard.NameTooLong",
        description: "Dashboard name must not exceed 256 characters");

    public static Error NameAlreadyExists => Error.Conflict(
        code: "Dashboard.NameAlreadyExists",
        description: "A dashboard with the same name already exists");
}
