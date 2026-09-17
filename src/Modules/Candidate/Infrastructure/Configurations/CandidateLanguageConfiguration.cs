using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Configurations;

public sealed class CandidateLanguageConfiguration : IEntityTypeConfiguration<CandidateLanguage>
{
    public void Configure(EntityTypeBuilder<CandidateLanguage> builder)
    {
        builder.ToTable("CandidateLanguages");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.CandidateCvContentId).IsRequired();

        builder.Property(l => l.LanguageId).IsRequired();
        builder.Property(l => l.LanguageLevelId).IsRequired();
        builder.Property(l => l.IsNativeLanguage).IsRequired();
    }
}
