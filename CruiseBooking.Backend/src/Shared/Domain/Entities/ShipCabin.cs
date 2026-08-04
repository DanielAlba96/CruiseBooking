namespace Shared.Domain.Entities;

public class ShipCabin
{
    public int Id { get; set; }
    public int ShipId { get; set; }
    public int CabinTypeId { get; set; }
    public int Number { get; set; }
    public decimal Price { get; set; }

    public Ship? Ship { get; set; }
    public CabinType? CabinType { get; set; }

    public ICollection<BookingCabin> BookingCabins { get; set; } = [];
}
