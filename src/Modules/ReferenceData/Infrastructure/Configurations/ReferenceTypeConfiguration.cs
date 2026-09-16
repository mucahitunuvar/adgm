using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class ReferenceTypeConfiguration : LookupItemConfiguration<ReferenceType>
{
    protected override string TableName => "ReferenceTypes";
}
