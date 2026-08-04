namespace Shared.Domain.Entities;

public class UserPayment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string CardId { get; set; }
    public required string CardBrand { get; set; }
    public required string LastFourDigits { get; set; }
    public required string CardHolderName { get; set; }
    public int ExpirationMonth { get; set; }
    public int ExpirationYear { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
