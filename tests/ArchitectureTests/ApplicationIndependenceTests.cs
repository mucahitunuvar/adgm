using NetArchTest.Rules;

namespace GenclikMerkezi.ArchitectureTests;

// AGENTS.md §7: Application must not depend on concrete Infrastructure implementations.
// In this codebase the Application layer spans both the "<Module>.Application" (abstractions)
// and "<Module>.Features" (use cases) namespace roots (Vertical Slice Architecture, ADR-005).
public class ApplicationIndependenceTests
{
    [Fact]
    public void Application_Should_Not_DependOn_Infrastructure()
    {
        var violations = new List<string>();

        foreach (var assembly in ModuleAssemblies.All)
        {
            var moduleNamespace = assembly.GetName().Name!;

            var result = Types.InAssembly(assembly)
                .That().ResideInNamespaceContaining($"{moduleNamespace}.Application")
                .Or().ResideInNamespaceContaining($"{moduleNamespace}.Features")
                .ShouldNot().HaveDependencyOnAny($"{moduleNamespace}.Infrastructure")
                .GetResult();

            if (!result.IsSuccessful)
            {
                violations.Add($"{moduleNamespace}.Application/Features -> {string.Join(", ", result.FailingTypeNames ?? [])}");
            }
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }
}
