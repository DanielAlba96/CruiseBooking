namespace Shared.Domain.Entities;

public class CabinType
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int MaxOccupancy { get; set; }

    public ICollection<ShipCabin> ShipCabins { get; set; } = [];
}
