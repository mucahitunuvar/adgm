using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Configurations;

public sealed class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("Certificates");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.CandidateCvContentId).IsRequired();

        builder.Property(c => c.Name).HasMaxLength(300).IsRequired();
        builder.Property(c => c.IssuingInstitution).HasMaxLength(300).IsRequired();
        builder.Property(c => c.CertificateDate);
        builder.Property(c => c.Description).HasMaxLength(2000);
    }
}
