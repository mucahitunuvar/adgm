using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class DisabilityCategoryConfiguration : LookupItemConfiguration<DisabilityCategory>
{
    protected override string TableName => "DisabilityCategories";
}
