using System.Reflection;
using NetArchTest.Rules;

namespace GenclikMerkezi.ArchitectureTests;

// ADR-024 §1: unlike every other module (which may depend on several GenclikMerkezi.Contracts.*
// namespaces per ADR-016 Decision 2), Website must stay portable across projects and therefore may
// depend on nothing under GenclikMerkezi.Contracts.* except its own (currently empty)
// GenclikMerkezi.Contracts.Website namespace. This is Website-specific, so it is a dedicated test
// rather than folded into ModuleBoundaryTests' generic all-modules-vs-all-modules loop.
public class WebsiteContractsBoundaryTests
{
    [Fact]
    public void Website_Should_Not_DependOn_OtherModulesContracts()
    {
        var websiteAssembly = ModuleAssemblies.All.Single(
            assembly => assembly.GetName().Name == "GenclikMerkezi.Modules.Website");
        var contractsAssembly = Assembly.Load("GenclikMerkezi.Contracts");

        var forbiddenNamespaces = contractsAssembly.SafeGetTypes()
            .Select(type => type.Namespace)
            .Where(ns => ns is not null && ns.StartsWith("GenclikMerkezi.Contracts.", StringComparison.Ordinal))
            .Select(ns => ns!)
            .Where(ns => ns != "GenclikMerkezi.Contracts.Website"
                && !ns.StartsWith("GenclikMerkezi.Contracts.Website.", StringComparison.Ordinal))
            .Distinct()
            .ToArray();

        var result = Types.InAssembly(websiteAssembly)
            .ShouldNot().HaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }
}
