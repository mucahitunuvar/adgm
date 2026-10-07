using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Configurations;

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");

        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id).ValueGeneratedNever();

        builder.Property(j => j.CompanyId).IsRequired();
        builder.HasIndex(j => j.CompanyId);

        builder.Property(j => j.Title).HasMaxLength(200).IsRequired();

        // Görev 1 (master prompt): nullable (taslakta null), ilk yayında atanır ve değişmez. Id eki
        // çakışmayı pratikte imkânsız kılar; filtreli benzersiz indeks emniyet kemeridir.
        builder.Property(j => j.Slug).HasMaxLength(120);
        builder.HasIndex(j => j.Slug).IsUnique().HasFilter("[Slug] IS NOT NULL");

        builder.Property(j => j.IsForDisabledCandidates).IsRequired();
        builder.Property(j => j.EmploymentTypeId).IsRequired();
        builder.Property(j => j.WorkLocationTypeId).IsRequired();
        builder.Property(j => j.PositionId).IsRequired();
        builder.Property(j => j.DepartmentId).IsRequired();
        builder.Property(j => j.ProvinceId).IsRequired();
        builder.Property(j => j.DescriptionHtml).HasMaxLength(4000);
        builder.Property(j => j.ExperienceLevelId).IsRequired();

        builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(50).IsRequired();

        // Görev 3 (Employer public jobs master prompt): public ilan listesi/detayı ve Görev 4'ün
        // arama adaptörü dahil her public sorgu önce Status == Published'a daralır, sonra
        // PublishedAtUtc DESC sıralar (liste) ya da artan sıralar (Görev 4). CompanyId/Slug'ın aksine
        // bu filtrenin seçiciliği düşük görünebilir ama her satırda uygulanan ortak önkoşul olduğu ve
        // varsayılan sıralamayı da kapsadığı için eklendi - diğer filtre alanları (provinceId vb.)
        // isteğe bağlı/seyrek kullanıldığından ölçülmüş bir ihtiyaç olmadan eklenmedi (PERFORMANCE.md
        // "ölç, sonra ekle").
        builder.HasIndex(j => new { j.Status, j.PublishedAtUtc });
        builder.Property(j => j.ReviewedByAdvisorId);
        builder.Property(j => j.ReviewedAtUtc);
        builder.Property(j => j.RejectionReason).HasMaxLength(1000);
        builder.Property(j => j.RevisionNotes).HasMaxLength(1000);
        builder.Property(j => j.PublishedAtUtc);
        builder.Property(j => j.SuspendedByUserId);
        builder.Property(j => j.SuspendedAtUtc);
        builder.Property(j => j.SuspensionReason).HasMaxLength(1000);
        builder.Property(j => j.CreatedAtUtc).IsRequired();

        builder.HasMany(j => j.GenderPreferences)
            .WithOne()
            .HasForeignKey(p => p.JobId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(j => j.GenderPreferences).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(j => j.MilitaryStatusPreferences)
            .WithOne()
            .HasForeignKey(p => p.JobId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(j => j.MilitaryStatusPreferences).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(j => j.EducationLevelPreferences)
            .WithOne()
            .HasForeignKey(p => p.JobId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(j => j.EducationLevelPreferences).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(j => j.DrivingLicensePreferences)
            .WithOne()
            .HasForeignKey(p => p.JobId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(j => j.DrivingLicensePreferences).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(j => j.LanguageRequirements)
            .WithOne()
            .HasForeignKey(r => r.JobId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(j => j.LanguageRequirements).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
