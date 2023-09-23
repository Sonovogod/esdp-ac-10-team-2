using Application.Models.EnergyResult;
using Domain.Entities;
using FluentValidation;

namespace Application.Validation;

public class TotalFluxDensityResultValidator : AbstractValidator<TotalFluxDensity>
{
    public TotalFluxDensityResultValidator()
    {
        RuleFor(totalFlux => totalFlux.Distance)
            .NotEqual(0).WithMessage("Значение дистанции не может быть 0");
        RuleFor(totalFlux => totalFlux.Value)
            .NotEqual(0).WithMessage("Значение не может быть 0");
    }
}