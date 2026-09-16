using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class SkillConfiguration : LookupItemConfiguration<Skill>
{
    protected override string TableName => "Skills";
}
