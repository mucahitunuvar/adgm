using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class ExperienceLevelConfiguration : LookupItemConfiguration<ExperienceLevel>
{
    protected override string TableName => "ExperienceLevels";
}
