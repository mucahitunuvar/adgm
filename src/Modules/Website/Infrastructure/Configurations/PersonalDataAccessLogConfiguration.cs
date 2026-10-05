using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Configurations;

public sealed class PersonalDataAccessLogConfiguration : IEntityTypeConfiguration<PersonalDataAccessLog>
{
    public void Configure(EntityTypeBuilder<PersonalDataAccessLog> builder)
    {
        builder.ToTable("PersonalDataAccessLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.AccessedAtUtc).IsRequired();
        builder.HasIndex(a => a.AccessedAtUtc);

        builder.Property(a => a.EntityType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(a => a.EntityId);
        builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Detail).HasMaxLength(PersonalDataAccessLog.MaxDetailLength);
    }
}
