namespace Shared.Domain.Entities;

public class BookingCabinOccupant
{
    public int Id { get; set; }
    public int BookingCabinId { get; set; }
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public DateTime BirthDate { get; set; }
    public string Dni { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Address { get; set; } = null!;

    public BookingCabin? BookingCabin { get; set; }
}
