using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Persistence.Configurators;

internal class CruiseDateConfiguration : IEntityTypeConfiguration<CruiseDate>
{
    public void Configure(EntityTypeBuilder<CruiseDate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PriceModifier)
               .HasPrecision(18, 2);

        builder.HasMany(x => x.CruiseDateExtras)
            .WithOne(x => x.CruiseDate)
            .HasForeignKey(x => x.CruiseDateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
