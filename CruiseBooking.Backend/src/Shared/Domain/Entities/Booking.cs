namespace Shared.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int CruiseDateId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal ChargeAmount { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? ChargeId { get; set; }
    public DateTime? ChargedAt { get; set; }
    public string? RefundId { get; set; }
    public DateTime? RefundedAt { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Created;
    public string? CheckInTokenHash { get; set; }

    public CruiseDate? CruiseDate { get; set; }
    public User? User { get; set; }
    public ICollection<BookingCabin> Cabins { get; set; } = [];
    public ICollection<BookingExtra> Extras { get; set; } = [];
}
