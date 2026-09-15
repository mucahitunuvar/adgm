using NetArchTest.Rules;

namespace GenclikMerkezi.ArchitectureTests;

// AGENTS.md §7: Domain must not depend on Application, Infrastructure, EF Core, ASP.NET Core or MediatR.
public class DomainIndependenceTests
{
    [Fact]
    public void Domain_Should_Not_DependOn_OuterLayersOrFrameworks()
    {
        var violations = new List<string>();

        foreach (var assembly in ModuleAssemblies.All)
        {
            var moduleNamespace = assembly.GetName().Name!;

            var result = Types.InAssembly(assembly)
                .That().ResideInNamespaceContaining($"{moduleNamespace}.Domain")
                .ShouldNot().HaveDependencyOnAny(
                    $"{moduleNamespace}.Application",
                    $"{moduleNamespace}.Features",
                    $"{moduleNamespace}.Infrastructure",
                    "Microsoft.EntityFrameworkCore",
                    "Microsoft.AspNetCore",
                    "MediatR")
                .GetResult();

            if (!result.IsSuccessful)
            {
                violations.Add($"{moduleNamespace}.Domain -> {string.Join(", ", result.FailingTypeNames ?? [])}");
            }
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }
}
