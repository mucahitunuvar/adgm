using System.Reflection;
using NetArchTest.Rules;

namespace GenclikMerkezi.ArchitectureTests;

// Görev 4 (Employer public jobs master prompt): ADR-016 Decision 2 (Option C) publishes contracts
// (IPublishedJobModuleContract/PublishedJobSummary dahil) as plain interfaces/DTOs other modules
// call in-process - the whole point breaks if a contract type leaks an owning module's internal
// Domain/Application/Features/Infrastructure type into its own signature. GenclikMerkezi.Contracts
// has no ProjectReference to any Modules.* project (see its own .csproj - only SharedKernel), so
// this is already a compile-time guarantee; this test documents and guards that invariant
// explicitly, the way the master prompt's "mimari test" item asks for.
public class ContractsModuleIndependenceTests
{
    [Fact]
    public void Contracts_Should_Not_DependOn_AnyModule()
    {
        var contractsAssembly = Assembly.Load("GenclikMerkezi.Contracts");
        var moduleNames = ModuleAssemblies.All.Select(assembly => assembly.GetName().Name!).ToArray();

        var result = Types.InAssembly(contractsAssembly)
            .ShouldNot().HaveDependencyOnAny(moduleNames)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }
}
