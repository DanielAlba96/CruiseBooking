using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Persistence.Configurators;

internal class ShipCabinConfiguration : IEntityTypeConfiguration<ShipCabin>
{
    public void Configure(EntityTypeBuilder<ShipCabin> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price).HasPrecision(18, 2);

        builder.HasOne(x => x.CabinType)
            .WithMany(x => x.ShipCabins)
            .HasForeignKey(x => x.CabinTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
