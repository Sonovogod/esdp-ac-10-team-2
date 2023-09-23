using Application.Models.EnergyResult;
using FluentValidation;

namespace Application.Validation;

public class EnergyResultValidator : AbstractValidator<CreateEnergyResultDto>
{
    public EnergyResultValidator()
    {
        RuleFor(energyResult => energyResult.Gain)
            .NotEqual(0).WithMessage("Значение коэфициента усиления не может быть 0");
        RuleFor(energyResult => energyResult.PowerSignal)
            .NotEqual(0).WithMessage("Значение мощности сигнала не может быть 0");
        RuleFor(energyResult => energyResult.TransmitLossFactor)
            .NotEqual(0).WithMessage("Значение ослабление сигнала передатчика не может быть 0");
        RuleFor(energyResult => energyResult.HeightInstall)
            .NotEqual(0).WithMessage("Значение высоты установки не может быть 0");
    }
}