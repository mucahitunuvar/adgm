using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class GenderConfiguration : LookupItemConfiguration<Gender>
{
    protected override string TableName => "Genders";
}
