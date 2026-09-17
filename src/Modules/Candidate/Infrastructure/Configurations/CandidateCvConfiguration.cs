using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Configurations;

public sealed class CandidateCvConfiguration : IEntityTypeConfiguration<CandidateCv>
{
    public void Configure(EntityTypeBuilder<CandidateCv> builder)
    {
        builder.ToTable("CandidateCvs");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.UserId).IsRequired();
        builder.HasIndex(c => c.UserId).IsUnique();

        // İletişim Bilgileri
        builder.OwnsOne(c => c.Photo, photo =>
        {
            photo.ToTable("CandidateCvs");
            photo.Property(p => p.FileKey).HasColumnName("PhotoFileKey").HasMaxLength(500);
            photo.Property(p => p.OriginalFileName).HasColumnName("PhotoOriginalFileName").HasMaxLength(260);
            photo.Property(p => p.ContentType).HasColumnName("PhotoContentType").HasMaxLength(100);
            photo.Property(p => p.SizeInBytes).HasColumnName("PhotoSizeInBytes");
            photo.Property(p => p.UploadedAtUtc).HasColumnName("PhotoUploadedAtUtc");
            photo.Property(p => p.OwnerEntityType).HasColumnName("PhotoOwnerEntityType").HasMaxLength(100);
            photo.Property(p => p.OwnerEntityId).HasColumnName("PhotoOwnerEntityId");
        });

        builder.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.LastName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
        builder.Property(c => c.PhoneNumber).HasMaxLength(20);
        builder.Property(c => c.CountryId);
        builder.Property(c => c.ProvinceId);
        builder.Property(c => c.DistrictId);
        builder.Property(c => c.Address).HasMaxLength(500);

        builder.HasMany(c => c.SocialMediaLinks)
            .WithOne()
            .HasForeignKey(l => l.CandidateCvId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.SocialMediaLinks).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Kişisel Bilgiler
        builder.Property(c => c.Title).HasMaxLength(200);
        builder.Property(c => c.GenderId);
        builder.Property(c => c.BirthDate);
        builder.Property(c => c.DriversLicenseTypeId);
        builder.Property(c => c.NationalityId);
        builder.Property(c => c.NetSalaryExpectation).HasColumnType("decimal(18,2)");
        builder.Property(c => c.MilitaryStatusId);

        builder.OwnsOne(c => c.DisabilityInfo, disability =>
        {
            disability.ToTable("CandidateCvs");
            disability.Property(d => d.CategoryId).HasColumnName("DisabilityCategoryId");
            disability.Property(d => d.Percentage).HasColumnName("DisabilityPercentage");
            disability.Property(d => d.Description).HasColumnName("DisabilityDescription").HasMaxLength(2000);
            disability.Property(d => d.HasHealthReport).HasColumnName("DisabilityHasHealthReport");
            disability.Property(d => d.UsesMedication).HasColumnName("DisabilityUsesMedication");
            disability.Property(d => d.HasChronicCondition).HasColumnName("DisabilityHasChronicCondition");
            disability.Property(d => d.HasContagiousDisease).HasColumnName("DisabilityHasContagiousDisease");
            disability.Property(d => d.HasConsciousnessLossRisk).HasColumnName("DisabilityHasConsciousnessLossRisk");
        });

        // Sistem alanları
        builder.Property(c => c.CareerAdvisorId);
        builder.Property(c => c.CompletionPercentage).IsRequired();
    }
}
