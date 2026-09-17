using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class EmploymentTypeConfiguration : LookupItemConfiguration<EmploymentType>
{
    protected override string TableName => "EmploymentTypes";

    public override void Configure(EntityTypeBuilder<EmploymentType> builder)
    {
        base.Configure(builder);

        builder.HasData(EmploymentTypeSeedData.All.Select((item, index) => new
        {
            Id = DeterministicGuid.Create($"EmploymentType:{item.Code}"),
            item.Code,
            DisplayName = item.Name,
            IsActive = true,
            SortOrder = index,
        }));
    }
}
