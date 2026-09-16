using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class LanguageConfiguration : LookupItemConfiguration<Language>
{
    protected override string TableName => "Languages";
}
