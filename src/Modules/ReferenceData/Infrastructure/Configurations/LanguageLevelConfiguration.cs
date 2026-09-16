using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class LanguageLevelConfiguration : LookupItemConfiguration<LanguageLevel>
{
    protected override string TableName => "LanguageLevels";
}
