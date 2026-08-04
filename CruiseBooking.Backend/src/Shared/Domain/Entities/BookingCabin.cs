namespace Shared.Domain.Entities;

public class BookingCabin
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int CabinId { get; set; }
    public decimal Price { get; set; }
    public int Occupants { get; set; }

    public Booking? Booking { get; set; }
    public ShipCabin? Cabin { get; set; }
    public ICollection<BookingCabinOccupant> OccupantsDetails { get; set; } = [];
}
