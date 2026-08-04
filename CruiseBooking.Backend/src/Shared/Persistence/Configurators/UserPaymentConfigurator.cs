using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Persistence.Configurators;

internal class UserPaymentConfigurator : IEntityTypeConfiguration<UserPayment>
{
    public void Configure(EntityTypeBuilder<UserPayment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LastFourDigits).HasMaxLength(4);

        builder.HasOne(x => x.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.Active })
            .HasFilter("\"Active\" = true")
            .IsUnique();
    }
}
