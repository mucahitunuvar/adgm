using GenclikMerkezi.Modules.ReferenceData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class TaxOfficeConfiguration : LookupItemConfiguration<TaxOffice>
{
    protected override string TableName => "TaxOffices";

    public override void Configure(EntityTypeBuilder<TaxOffice> builder)
    {
        base.Configure(builder);

        builder.Property(t => t.ProvinceId).IsRequired();
        builder.HasIndex(t => t.ProvinceId);
    }
}
