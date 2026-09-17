using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class WorkLocationTypeConfiguration : LookupItemConfiguration<WorkLocationType>
{
    protected override string TableName => "WorkLocationTypes";

    public override void Configure(EntityTypeBuilder<WorkLocationType> builder)
    {
        base.Configure(builder);

        builder.HasData(WorkLocationTypeSeedData.All.Select((item, index) => new
        {
            Id = DeterministicGuid.Create($"WorkLocationType:{item.Code}"),
            item.Code,
            DisplayName = item.Name,
            IsActive = true,
            SortOrder = index,
        }));
    }
}
