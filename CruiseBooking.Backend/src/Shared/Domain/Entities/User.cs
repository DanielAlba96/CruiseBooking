namespace Shared.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public Guid KeycloakUserGuid { get; set; }
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public DateTime BirthDate { get; set; }
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public Address Address { get; set; } = null!;
    public string? CustomerId { get; set; }

    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<UserPayment> Payments { get; set; } = [];
}

public class Address
{
    public string Street { get; set; } = null!;
    public string Locality { get; set; } = null!;
    public string Region { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
}
