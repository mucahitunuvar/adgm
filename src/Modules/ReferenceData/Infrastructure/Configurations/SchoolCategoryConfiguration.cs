using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class SchoolCategoryConfiguration : LookupItemConfiguration<SchoolCategory>
{
    protected override string TableName => "SchoolCategories";
}
