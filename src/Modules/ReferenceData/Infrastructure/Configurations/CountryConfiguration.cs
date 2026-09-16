using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class CountryConfiguration : LookupItemConfiguration<Country>
{
    protected override string TableName => "Countries";

    public override void Configure(EntityTypeBuilder<Country> builder)
    {
        base.Configure(builder);

        builder.HasData(CountrySeedData.All.Select((country, index) => new
        {
            Id = DeterministicGuid.Create($"Country:{country.Code}"),
            country.Code,
            DisplayName = country.Name,
            IsActive = true,
            SortOrder = index,
        }));
    }
}
