using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Persistence.Configurators;

internal class BookingCabinConfiguration : IEntityTypeConfiguration<BookingCabin>
{
    public void Configure(EntityTypeBuilder<BookingCabin> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price).HasPrecision(18, 2);

        builder.HasMany(x => x.OccupantsDetails)
               .WithOne(x => x.BookingCabin)
               .HasForeignKey(x => x.BookingCabinId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cabin)
               .WithMany(x => x.BookingCabins)
               .HasForeignKey(x => x.CabinId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
