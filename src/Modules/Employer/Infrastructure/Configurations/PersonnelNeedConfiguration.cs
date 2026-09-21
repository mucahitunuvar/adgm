using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Configurations;

public sealed class PersonnelNeedConfiguration : IEntityTypeConfiguration<PersonnelNeed>
{
    public void Configure(EntityTypeBuilder<PersonnelNeed> builder)
    {
        builder.ToTable("PersonnelNeeds");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.CompanyId).IsRequired();
        builder.HasIndex(p => p.CompanyId);

        builder.Property(p => p.EmploymentTypeId).IsRequired();
        builder.Property(p => p.WorkLocationTypeId).IsRequired();
        builder.Property(p => p.PositionId).IsRequired();
        builder.Property(p => p.DepartmentId).IsRequired();
        builder.Property(p => p.Quantity).IsRequired();
        builder.Property(p => p.ProvinceId).IsRequired();
        builder.Property(p => p.ExperienceLevelId).IsRequired();
        builder.Property(p => p.DetailsText).HasMaxLength(4000);

        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(p => p.PooledByAdvisorId);
        builder.Property(p => p.PooledAtUtc);
        builder.Property(p => p.ClosedByAdvisorId);
        builder.Property(p => p.ClosedAtUtc);
        builder.Property(p => p.FulfilledByCandidateCvId);
        builder.Property(p => p.CreatedAtUtc).IsRequired();

        builder.HasMany(p => p.GenderPreferences)
            .WithOne()
            .HasForeignKey(p => p.PersonnelNeedId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.GenderPreferences).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.MilitaryStatusPreferences)
            .WithOne()
            .HasForeignKey(p => p.PersonnelNeedId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.MilitaryStatusPreferences).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.EducationLevelPreferences)
            .WithOne()
            .HasForeignKey(p => p.PersonnelNeedId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.EducationLevelPreferences).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.DrivingLicensePreferences)
            .WithOne()
            .HasForeignKey(p => p.PersonnelNeedId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.DrivingLicensePreferences).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
