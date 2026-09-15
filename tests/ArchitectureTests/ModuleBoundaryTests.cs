using NetArchTest.Rules;

namespace GenclikMerkezi.ArchitectureTests;

// AGENTS.md §9/§12: a module must never reference another module's Domain/Application/Features/
// Infrastructure types directly. Only that module's Contracts/ namespace may be shared.
// Forbidding the specific internal-layer namespaces (rather than the module's root namespace)
// means "<Other>.Contracts" is never accidentally matched as a violation.
public class ModuleBoundaryTests
{
    private static readonly string[] InternalLayers = ["Domain", "Application", "Features", "Infrastructure"];

    [Fact]
    public void Modules_Should_Not_DependOn_OtherModulesInternalLayers()
    {
        var modules = ModuleAssemblies.All;
        var violations = new List<string>();

        foreach (var assembly in modules)
        {
            var ownName = assembly.GetName().Name!;

            foreach (var otherAssembly in modules)
            {
                var otherName = otherAssembly.GetName().Name!;

                if (otherName == ownName)
                {
                    continue;
                }

                var forbiddenNamespaces = InternalLayers.Select(layer => $"{otherName}.{layer}").ToArray();

                var result = Types.InAssembly(assembly)
                    .That().ResideInNamespaceStartingWith(ownName)
                    .ShouldNot().HaveDependencyOnAny(forbiddenNamespaces)
                    .GetResult();

                if (!result.IsSuccessful)
                {
                    violations.Add($"{ownName} -> {otherName}: {string.Join(", ", result.FailingTypeNames ?? [])}");
                }
            }
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }
}
