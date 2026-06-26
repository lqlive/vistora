using FluentValidation;
using Nexova.Charts.Errors;
using Nexova.Charts.Models;

namespace Nexova.Charts.Validators;

public sealed class ChartRequestValidator : AbstractValidator<ChartRequest>
{
    public ChartRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage(ChartErrors.NameRequired.Description)
            .WithErrorCode(ChartErrors.NameRequired.Code)
            .MaximumLength(256)
            .WithMessage(ChartErrors.NameTooLong.Description)
            .WithErrorCode(ChartErrors.NameTooLong.Code);

        RuleFor(request => request.VizType)
            .NotEmpty()
            .WithMessage(ChartErrors.VizTypeRequired.Description)
            .WithErrorCode(ChartErrors.VizTypeRequired.Code)
            .MaximumLength(64)
            .WithMessage(ChartErrors.VizTypeTooLong.Description)
            .WithErrorCode(ChartErrors.VizTypeTooLong.Code);

        RuleFor(request => request.Dataset)
            .NotEmpty()
            .WithMessage(ChartErrors.DatasetRequired.Description)
            .WithErrorCode(ChartErrors.DatasetRequired.Code)
            .MaximumLength(256)
            .WithMessage(ChartErrors.DatasetTooLong.Description)
            .WithErrorCode(ChartErrors.DatasetTooLong.Code);
    }
}
