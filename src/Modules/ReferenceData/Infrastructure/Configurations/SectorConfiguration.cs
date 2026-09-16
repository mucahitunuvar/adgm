using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class SectorConfiguration : LookupItemConfiguration<Sector>
{
    protected override string TableName => "Sectors";
}
