using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class SchoolCategoryConfiguration : LookupItemConfiguration<SchoolCategory>
{
    protected override string TableName => "SchoolCategories";

    public override void Configure(EntityTypeBuilder<SchoolCategory> builder)
    {
        base.Configure(builder);

        builder.HasData(SchoolCategorySeedData.All.Select((item, index) => new
        {
            Id = DeterministicGuid.Create($"SchoolCategory:{item.Code}"),
            item.Code,
            DisplayName = item.Name,
            IsActive = true,
            SortOrder = index,
        }));
    }
}
