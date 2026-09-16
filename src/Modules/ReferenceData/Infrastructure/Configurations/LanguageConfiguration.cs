using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class LanguageConfiguration : LookupItemConfiguration<Language>
{
    protected override string TableName => "Languages";

    public override void Configure(EntityTypeBuilder<Language> builder)
    {
        base.Configure(builder);

        builder.HasData(LanguageSeedData.All.Select((language, index) => new
        {
            Id = DeterministicGuid.Create($"Language:{language.Code}"),
            language.Code,
            DisplayName = language.Name,
            IsActive = true,
            SortOrder = index,
        }));
    }
}
