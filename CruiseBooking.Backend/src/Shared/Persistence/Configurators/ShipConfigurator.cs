using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Persistence.Configurators;

internal class ShipConfiguration : IEntityTypeConfiguration<Ship>
{
    public void Configure(EntityTypeBuilder<Ship> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).IsRequired();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.Company).IsRequired();

        builder.HasMany(x => x.CruiseDates)
               .WithOne(x => x.Ship)
               .HasForeignKey(x => x.ShipId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Cabins)
               .WithOne(x => x.Ship)
               .HasForeignKey(x => x.ShipId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
