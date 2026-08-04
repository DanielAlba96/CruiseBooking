using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Persistence.Configurators;

internal class CabinTypeConfiguration : IEntityTypeConfiguration<CabinType>
{
    public void Configure(EntityTypeBuilder<CabinType> builder)
    {
        builder.HasKey(x => x.Id);
    }
}
