using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Persistence.Configurators;

internal class BookingCabinOccupantConfiguration : IEntityTypeConfiguration<BookingCabinOccupant>
{
    public void Configure(EntityTypeBuilder<BookingCabinOccupant> builder)
    {
        builder.HasKey(x => x.Id);
    }
}
