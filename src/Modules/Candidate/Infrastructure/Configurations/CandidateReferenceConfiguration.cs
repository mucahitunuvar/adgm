using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Configurations;

public sealed class CandidateReferenceConfiguration : IEntityTypeConfiguration<CandidateReference>
{
    public void Configure(EntityTypeBuilder<CandidateReference> builder)
    {
        builder.ToTable("CandidateReferences");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.CandidateCvContentId).IsRequired();

        builder.Property(r => r.ReferenceTypeId).IsRequired();
        builder.Property(r => r.ReferenceLanguageId).IsRequired();
        builder.Property(r => r.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(r => r.LastName).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Company).HasMaxLength(300);
        builder.Property(r => r.Position).HasMaxLength(200);
        builder.Property(r => r.Email).HasMaxLength(256);
        builder.Property(r => r.PhoneNumber).HasMaxLength(20);
    }
}
