using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class ReferenceTypeConfiguration : LookupItemConfiguration<ReferenceType>
{
    protected override string TableName => "ReferenceTypes";

    public override void Configure(EntityTypeBuilder<ReferenceType> builder)
    {
        base.Configure(builder);

        builder.HasData(ReferenceTypeSeedData.All.Select((item, index) => new
        {
            Id = DeterministicGuid.Create($"ReferenceType:{item.Code}"),
            item.Code,
            DisplayName = item.Name,
            IsActive = true,
            SortOrder = index,
        }));
    }
}
