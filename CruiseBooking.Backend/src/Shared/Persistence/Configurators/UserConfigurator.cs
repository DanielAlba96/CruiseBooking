using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Persistence.Configurators;

internal class UserConfigurator : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Email);

        builder.OwnsOne(x => x.Address, a =>
        {
            a.Property(p => p.Street).HasColumnName("Address_Street");
            a.Property(p => p.Locality).HasColumnName("Address_Locality");
            a.Property(p => p.Region).HasColumnName("Address_Region");
            a.Property(p => p.PostalCode).HasColumnName("Address_PostalCode");
            a.Property(p => p.Country).HasColumnName("Address_Country");
        });
    }
}
