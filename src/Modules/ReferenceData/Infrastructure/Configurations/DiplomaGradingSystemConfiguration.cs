using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class DiplomaGradingSystemConfiguration : LookupItemConfiguration<DiplomaGradingSystem>
{
    protected override string TableName => "DiplomaGradingSystems";

    public override void Configure(EntityTypeBuilder<DiplomaGradingSystem> builder)
    {
        base.Configure(builder);

        builder.HasData(DiplomaGradingSystemSeedData.All.Select((item, index) => new
        {
            Id = DeterministicGuid.Create($"DiplomaGradingSystem:{item.Code}"),
            item.Code,
            DisplayName = item.Name,
            IsActive = true,
            SortOrder = index,
        }));
    }
}
