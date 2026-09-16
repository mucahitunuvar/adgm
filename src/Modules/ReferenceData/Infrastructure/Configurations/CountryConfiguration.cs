using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class CountryConfiguration : LookupItemConfiguration<Country>
{
    protected override string TableName => "Countries";
}
