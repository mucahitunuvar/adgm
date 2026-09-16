using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class MilitaryStatusConfiguration : LookupItemConfiguration<MilitaryStatus>
{
    protected override string TableName => "MilitaryStatuses";
}
