using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Configurations;

public sealed class CandidateCvContentConfiguration : IEntityTypeConfiguration<CandidateCvContent>
{
    public void Configure(EntityTypeBuilder<CandidateCvContent> builder)
    {
        builder.ToTable("CandidateCvContents");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.CandidateCvId).IsRequired();
        builder.HasIndex(c => c.CandidateCvId).IsUnique();

        builder.Property(c => c.Summary).HasMaxLength(4000);
        builder.Property(c => c.ComputerSkills).HasMaxLength(2000);
        builder.Property(c => c.Hobbies).HasMaxLength(2000);

        builder.OwnsOne(c => c.CvFile, cvFile =>
        {
            cvFile.ToTable("CandidateCvContents");
            cvFile.Property(f => f.FileKey).HasColumnName("CvFileKey").HasMaxLength(500);
            cvFile.Property(f => f.OriginalFileName).HasColumnName("CvFileOriginalFileName").HasMaxLength(260);
            cvFile.Property(f => f.ContentType).HasColumnName("CvFileContentType").HasMaxLength(100);
            cvFile.Property(f => f.SizeInBytes).HasColumnName("CvFileSizeInBytes");
            cvFile.Property(f => f.UploadedAtUtc).HasColumnName("CvFileUploadedAtUtc");
            cvFile.Property(f => f.OwnerEntityType).HasColumnName("CvFileOwnerEntityType").HasMaxLength(100);
            cvFile.Property(f => f.OwnerEntityId).HasColumnName("CvFileOwnerEntityId");
        });

        builder.HasMany(c => c.Experiences)
            .WithOne()
            .HasForeignKey(e => e.CandidateCvContentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Experiences).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.Educations)
            .WithOne()
            .HasForeignKey(e => e.CandidateCvContentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Educations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.Languages)
            .WithOne()
            .HasForeignKey(l => l.CandidateCvContentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Languages).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.Certificates)
            .WithOne()
            .HasForeignKey(cert => cert.CandidateCvContentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Certificates).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.References)
            .WithOne()
            .HasForeignKey(r => r.CandidateCvContentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.References).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
