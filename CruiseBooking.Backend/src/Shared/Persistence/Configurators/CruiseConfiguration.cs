using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Persistence.Configurators;

internal class CruiseConfiguration : IEntityTypeConfiguration<Cruise>
{
    public void Configure(EntityTypeBuilder<Cruise> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Code).IsRequired();
        builder.Property(x => x.Zone).IsRequired();
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.OriginPort).IsRequired();
        builder.Property(x => x.Itinerary).IsRequired();

        builder.HasIndex(x => x.Zone);
        builder.HasIndex(x => x.OriginPort);
        builder.HasIndex(x => x.AdultsOnly);

        builder.HasMany(x => x.CruiseDates)
               .WithOne(x => x.Cruise)
               .HasForeignKey(x => x.CruiseId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
