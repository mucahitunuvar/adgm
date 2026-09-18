using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.CareerAdvisor.Infrastructure.Configurations;

public sealed class CareerAdvisorConfiguration : IEntityTypeConfiguration<Domain.CareerAdvisor>
{
    public void Configure(EntityTypeBuilder<Domain.CareerAdvisor> builder)
    {
        builder.ToTable("CareerAdvisors");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.UserId).IsRequired();
        builder.HasIndex(c => c.UserId).IsUnique();

        builder.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.LastName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
        builder.Property(c => c.PhoneNumber).HasMaxLength(20);

        builder.Property(c => c.IsActive).IsRequired();
        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.DeactivatedAtUtc);
    }
}
