using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class EducationLevelConfiguration : LookupItemConfiguration<EducationLevel>
{
    protected override string TableName => "EducationLevels";
}
