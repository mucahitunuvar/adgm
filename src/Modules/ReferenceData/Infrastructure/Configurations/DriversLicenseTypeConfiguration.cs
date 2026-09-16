using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class DriversLicenseTypeConfiguration : LookupItemConfiguration<DriversLicenseType>
{
    protected override string TableName => "DriversLicenseTypes";
}
