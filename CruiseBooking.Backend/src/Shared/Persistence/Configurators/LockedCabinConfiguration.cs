using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Persistence.Configurators;

internal class LockedCabinConfiguration : IEntityTypeConfiguration<LockedCabin>
{
    public void Configure(EntityTypeBuilder<LockedCabin> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price).HasPrecision(18, 2);

        builder.HasOne(x => x.CruiseDate)
               .WithMany()
               .HasForeignKey(x => x.CruiseDateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cabin)
               .WithMany()
               .HasForeignKey(x => x.CabinId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
