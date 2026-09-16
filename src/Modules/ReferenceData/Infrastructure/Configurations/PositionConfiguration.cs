using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class PositionConfiguration : LookupItemConfiguration<Position>
{
    protected override string TableName => "Positions";
}
