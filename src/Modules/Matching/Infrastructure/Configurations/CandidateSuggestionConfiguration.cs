using GenclikMerkezi.Modules.Matching.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Matching.Infrastructure.Configurations;

public sealed class CandidateSuggestionConfiguration : IEntityTypeConfiguration<CandidateSuggestion>
{
    public void Configure(EntityTypeBuilder<CandidateSuggestion> builder)
    {
        builder.ToTable("CandidateSuggestions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.PersonnelNeedId).IsRequired();
        builder.Property(s => s.CandidateCvId).IsRequired();
        builder.Property(s => s.SuggestingAdvisorId).IsRequired();

        // Bir (PersonnelNeed, CandidateCv) çiftine yalnızca bir öneri olabilir (yinelenen öneri
        // engeli) - CompanyConfiguration.TaxNumber'daki unique index deseniyle aynı savunma katmanı,
        // uygulama seviyesindeki ExistsForPersonnelNeedAndCandidateAsync kontrolünün yanında.
        builder.HasIndex(s => new { s.PersonnelNeedId, s.CandidateCvId }).IsUnique();

        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.DecidedByAdvisorId);
        builder.Property(s => s.DecidedAtUtc);
    }
}
