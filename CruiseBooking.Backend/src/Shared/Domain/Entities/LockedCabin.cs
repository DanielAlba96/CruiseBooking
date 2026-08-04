namespace Shared.Domain.Entities;

public class LockedCabin
{
    public int Id { get; set; }
    public int CruiseDateId { get; set; }
    public int CabinId { get; set; }
    public int UserId { get; set; }
    public int Occupants { get; set; }
    public decimal Price { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public bool IsBookingInProgress { get; set; } = false;

    public CruiseDate? CruiseDate { get; set; }
    public ShipCabin? Cabin { get; set; }
    public User? User { get; set; }
}
