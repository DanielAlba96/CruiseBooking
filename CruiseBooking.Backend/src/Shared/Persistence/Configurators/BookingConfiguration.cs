using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Persistence.Configurators;

internal class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChargeAmount).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasDefaultValue(BookingStatus.Created);
        builder.Property(x => x.CheckInTokenHash).HasMaxLength(64);

        builder.HasIndex(x => x.CheckInTokenHash).IsUnique();

        builder.HasOne(x => x.CruiseDate)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.CruiseDateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Cabins)
               .WithOne(x => x.Booking)
               .HasForeignKey(x => x.BookingId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Extras)
               .WithOne(x => x.Booking)
               .HasForeignKey(x => x.BookingId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
