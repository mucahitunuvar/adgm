using NetArchTest.Rules;

namespace GenclikMerkezi.ArchitectureTests;

// AGENTS.md §18: "Application handlers must not directly access EF Core." Feature handlers/endpoints
// must depend only on repository/UnitOfWork abstractions - never on DbContext or any EF Core type.
public class FeatureDbContextTests
{
    [Fact]
    public void Features_Should_Not_DependOn_EntityFrameworkCore()
    {
        var violations = new List<string>();

        foreach (var assembly in ModuleAssemblies.All)
        {
            var moduleNamespace = assembly.GetName().Name!;

            var result = Types.InAssembly(assembly)
                .That().ResideInNamespaceContaining($"{moduleNamespace}.Features")
                .ShouldNot().HaveDependencyOnAny("Microsoft.EntityFrameworkCore")
                .GetResult();

            if (!result.IsSuccessful)
            {
                violations.Add($"{moduleNamespace}.Features -> {string.Join(", ", result.FailingTypeNames ?? [])}");
            }
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }
}
