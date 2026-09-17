using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class WorkFieldConfiguration : LookupItemConfiguration<WorkField>
{
    protected override string TableName => "WorkFields";
}
