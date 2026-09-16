using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class NationalityConfiguration : LookupItemConfiguration<Nationality>
{
    protected override string TableName => "Nationalities";
}
