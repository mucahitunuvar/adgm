using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class DiplomaGradingSystemConfiguration : LookupItemConfiguration<DiplomaGradingSystem>
{
    protected override string TableName => "DiplomaGradingSystems";
}
