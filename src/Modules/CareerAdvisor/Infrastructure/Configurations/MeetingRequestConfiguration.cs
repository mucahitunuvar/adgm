using GenclikMerkezi.Modules.CareerAdvisor.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.CareerAdvisor.Infrastructure.Configurations;

public sealed class MeetingRequestConfiguration : IEntityTypeConfiguration<MeetingRequest>
{
    public void Configure(EntityTypeBuilder<MeetingRequest> builder)
    {
        builder.ToTable("MeetingRequests");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.CandidateCvId).IsRequired();
        builder.HasIndex(m => m.CandidateCvId);

        builder.Property(m => m.CandidateUserId).IsRequired();

        builder.Property(m => m.CareerAdvisorId).IsRequired();
        builder.HasIndex(m => m.CareerAdvisorId);

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.ProposedDateTimeUtc);
        builder.Property(m => m.ConfirmedAtUtc);
        builder.Property(m => m.CreatedAtUtc).IsRequired();
    }
}
