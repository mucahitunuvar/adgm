using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;
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

        var turkeyId = DeterministicGuid.Create("Country:TR");

        builder.HasData(ProvinceSeedData.All.Select((province, index) => new
        {
            Id = DeterministicGuid.Create($"Province:{province.Code}"),
            province.Code,
            DisplayName = province.Name,
            IsActive = true,
            SortOrder = index,
            CountryId = turkeyId,
        }));
    }
}
