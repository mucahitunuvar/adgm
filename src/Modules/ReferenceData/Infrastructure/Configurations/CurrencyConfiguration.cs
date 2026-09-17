using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class CurrencyConfiguration : LookupItemConfiguration<Currency>
{
    protected override string TableName => "Currencies";

    public override void Configure(EntityTypeBuilder<Currency> builder)
    {
        base.Configure(builder);

        builder.HasData(CurrencySeedData.All.Select((currency, index) => new
        {
            Id = DeterministicGuid.Create($"Currency:{currency.Code}"),
            currency.Code,
            DisplayName = currency.Name,
            IsActive = true,
            SortOrder = index,
        }));
    }
}
