namespace Shared.Domain.Entities;

public class CruiseDateExtra
{
    public int Id { get; set; }
    public int CruiseDateId { get; set; }
    public int ExtraId { get; set; }
    public decimal Price { get; set; }

    public CruiseDate? CruiseDate { get; set; }
    public Extra? Extra { get; set; }
}
