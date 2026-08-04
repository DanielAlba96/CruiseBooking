namespace Shared.Domain.Entities;

public class Ship
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Company { get; set; } = null!;

    public ICollection<CruiseDate> CruiseDates { get; set; } = [];
    public ICollection<ShipCabin> Cabins { get; set; } = [];
}
