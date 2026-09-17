using GenclikMerkezi.Modules.Candidate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Configurations;

public sealed class SocialMediaLinkConfiguration : IEntityTypeConfiguration<SocialMediaLink>
{
    public void Configure(EntityTypeBuilder<SocialMediaLink> builder)
    {
        builder.ToTable("SocialMediaLinks");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.CandidateCvId).IsRequired();

        builder.Property(l => l.Platform).HasMaxLength(100).IsRequired();
        builder.Property(l => l.Url).HasMaxLength(500).IsRequired();
    }
}
