using Nexova.Filters;

namespace Nexova.Extensions;

public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithValidation<T>(
        this RouteHandlerBuilder routeBuilder)
    {
        return routeBuilder
            .AddEndpointFilter<ValidationFilter<T>>()
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }
}
