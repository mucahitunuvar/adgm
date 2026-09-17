using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class DisabilityCategoryConfiguration : LookupItemConfiguration<DisabilityCategory>
{
    protected override string TableName => "DisabilityCategories";

    public override void Configure(EntityTypeBuilder<DisabilityCategory> builder)
    {
        base.Configure(builder);

        builder.HasData(DisabilityCategorySeedData.All.Select((item, index) => new
        {
            Id = DeterministicGuid.Create($"DisabilityCategory:{item.Code}"),
            item.Code,
            DisplayName = item.Name,
            IsActive = true,
            SortOrder = index,
        }));
    }
}
