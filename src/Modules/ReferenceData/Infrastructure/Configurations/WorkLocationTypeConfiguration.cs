using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class WorkLocationTypeConfiguration : LookupItemConfiguration<WorkLocationType>
{
    protected override string TableName => "WorkLocationTypes";
}
