using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.Configurations;

public sealed class DepartmentConfiguration : LookupItemConfiguration<Department>
{
    protected override string TableName => "Departments";
}
