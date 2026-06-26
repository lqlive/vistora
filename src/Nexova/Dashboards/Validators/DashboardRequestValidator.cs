using FluentValidation;
using Nexova.Dashboards.Errors;
using Nexova.Dashboards.Models;

namespace Nexova.Dashboards.Validators;

public sealed class DashboardRequestValidator : AbstractValidator<DashboardRequest>
{
    public DashboardRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage(DashboardErrors.NameRequired.Description)
            .WithErrorCode(DashboardErrors.NameRequired.Code)
            .MaximumLength(256)
            .WithMessage(DashboardErrors.NameTooLong.Description)
            .WithErrorCode(DashboardErrors.NameTooLong.Code);
    }
}
