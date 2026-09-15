namespace GenclikMerkezi.ArchitectureTests;

// ADR-009: generic repository abstractions (IRepository<T>, GenericRepository<T>) are forbidden.
// Repositories must be aggregate-specific (e.g. IUserRepository). Plain reflection is used here
// because generic-type-name matching is more reliable than NetArchTest's regex-based name filter.
public class NoGenericRepositoryTests
{
    [Fact]
    public void NoAssembly_Should_Define_GenericRepositoryAbstraction()
    {
        var assembliesToCheck = ModuleAssemblies.All
            .Append(typeof(GenclikMerkezi.SharedKernel.Domain.Entity).Assembly)
            .Append(typeof(GenclikMerkezi.BuildingBlocks.Infrastructure.Http.ResultHttpExtensions).Assembly);

        var violations = new List<string>();

        foreach (var assembly in assembliesToCheck)
        {
            foreach (var type in assembly.SafeGetTypes())
            {
                if (!type.IsGenericTypeDefinition)
                {
                    continue;
                }

                var simpleName = type.Name.Contains('`') ? type.Name[..type.Name.IndexOf('`')] : type.Name;

                if (simpleName is "IRepository" or "GenericRepository")
                {
                    violations.Add(type.FullName ?? type.Name);
                }
            }
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }
}
