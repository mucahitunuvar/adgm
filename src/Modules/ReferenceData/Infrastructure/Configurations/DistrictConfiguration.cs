using GenclikMerkezi.Modules.ReferenceData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class DistrictConfiguration : LookupItemConfiguration<District>
{
    protected override string TableName => "Districts";

    public override void Configure(EntityTypeBuilder<District> builder)
    {
        base.Configure(builder);

        builder.Property(d => d.ProvinceId).IsRequired();
        builder.HasIndex(d => d.ProvinceId);
    }
}
