using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Persistence.Configurators;

internal class BookingCabinExtraConfiguration : IEntityTypeConfiguration<BookingExtra>
{
    public void Configure(EntityTypeBuilder<BookingExtra> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price).HasPrecision(18, 2);

        builder.HasOne(x => x.Extra)
            .WithMany(x => x.BookingCabinExtras)
            .HasForeignKey(x => x.ExtraId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
