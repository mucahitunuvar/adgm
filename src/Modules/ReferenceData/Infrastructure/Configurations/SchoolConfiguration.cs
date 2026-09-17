using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class SchoolConfiguration : LookupItemConfiguration<School>
{
    protected override string TableName => "Schools";
}
