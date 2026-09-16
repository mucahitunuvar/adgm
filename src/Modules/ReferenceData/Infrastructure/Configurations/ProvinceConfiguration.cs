using GenclikMerkezi.Modules.ReferenceData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class ProvinceConfiguration : LookupItemConfiguration<Province>
{
    protected override string TableName => "Provinces";

    public override void Configure(EntityTypeBuilder<Province> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.CountryId).IsRequired();
        builder.HasIndex(p => p.CountryId);
    }
}
