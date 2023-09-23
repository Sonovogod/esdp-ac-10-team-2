using Domain.Entities;

namespace Application.Models.EnergyResult;

public class CreateTotalFluxDensityDto
{
    public int Distance { get; set; }
    public decimal Value { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; }
}