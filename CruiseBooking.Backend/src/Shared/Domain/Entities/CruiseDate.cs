using System.Text.Json.Serialization;

namespace Shared.Domain.Entities;

public class CruiseDate
{
    public int Id { get; set; }
    public int CruiseId { get; set; }
    public int ShipId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PaymentStartDate { get; set; }
    public DateTime CheckInStartDate { get; set; }

    [JsonIgnore]
    public decimal PriceModifier { get; set; }

    public Cruise? Cruise { get; set; }
    public Ship? Ship { get; set; }
    public ICollection<CruiseDateExtra> CruiseDateExtras { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
}
