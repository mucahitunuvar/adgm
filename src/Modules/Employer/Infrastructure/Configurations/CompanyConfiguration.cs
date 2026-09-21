using GenclikMerkezi.Modules.Employer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.UserId).IsRequired();
        builder.HasIndex(c => c.UserId).IsUnique();

        builder.Property(c => c.CareerAdvisorId);

        // Firma Bilgileri
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.SectorId).IsRequired();
        builder.Property(c => c.FoundedYear);
        builder.Property(c => c.EmployeeCount);
        builder.Property(c => c.WebsiteUrl).HasMaxLength(500);
        builder.Property(c => c.CountryId).IsRequired();
        builder.Property(c => c.ProvinceId).IsRequired();
        builder.Property(c => c.DistrictId).IsRequired();
        builder.Property(c => c.Address).HasMaxLength(500).IsRequired();
        builder.Property(c => c.AboutHtml).HasMaxLength(4000);

        // Hesap Bilgileri
        builder.Property(c => c.ContactFirstName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.ContactLastName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.ContactEmail).HasMaxLength(256).IsRequired();
        builder.Property(c => c.ContactPhone).HasMaxLength(20).IsRequired();
        builder.Property(c => c.TaxOfficeId).IsRequired();
        builder.Property(c => c.TaxNumber).HasMaxLength(10).IsRequired();
        builder.HasIndex(c => c.TaxNumber).IsUnique();
        builder.Property(c => c.MarketingConsent).IsRequired();

        // Sistem alanları
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(c => c.ApprovedByUserId);
        builder.Property(c => c.ApprovedAtUtc);
        builder.Property(c => c.RejectionReason).HasMaxLength(1000);
        builder.Property(c => c.DeactivatedByUserId);
        builder.Property(c => c.DeactivatedAtUtc);
        builder.Property(c => c.CreatedAtUtc).IsRequired();
    }
}
