
using Application.Models.Antennae;
using FluentValidation;

namespace Application.Validation;

public class AntennaValidator : AbstractValidator<AntennaDto>
{
    public AntennaValidator()
    {
        RuleFor(dto => dto.Model)
            .NotNull().WithMessage("Модель антенны не определена");
        RuleFor(dto => dto.VerticalSizeDiameter)
            .NotNull().WithMessage("Угол наклона антенны не задан");
    }
}