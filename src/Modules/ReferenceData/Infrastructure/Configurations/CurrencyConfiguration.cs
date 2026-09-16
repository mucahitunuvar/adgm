using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class CurrencyConfiguration : LookupItemConfiguration<Currency>
{
    protected override string TableName => "Currencies";
}
