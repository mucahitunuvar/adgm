using System.Reflection;

namespace GenclikMerkezi.ArchitectureTests;

// Discovers module assemblies by scanning this test project's own build output directory, so new
// modules are picked up automatically the moment their ProjectReference is added here - no test
// code changes needed. A ProjectReference always copies the referenced DLL to the output directory
// regardless of whether any type from it is actually used in source, unlike
// Assembly.GetReferencedAssemblies() (which reflects only the compiler-emitted AssemblyRef table -
// entries the compiler prunes unless a type from that assembly is actually referenced in code).
internal static class ModuleAssemblies
{
    public static IReadOnlyList<Assembly> All { get; } = DiscoverModuleAssemblies();

    private static IReadOnlyList<Assembly> DiscoverModuleAssemblies()
    {
        return Directory.GetFiles(AppContext.BaseDirectory, "GenclikMerkezi.Modules.*.dll")
            .Select(path => Assembly.Load(AssemblyName.GetAssemblyName(path)))
            .OrderBy(assembly => assembly.GetName().Name, StringComparer.Ordinal)
            .ToArray();
    }

    public static IEnumerable<Type> SafeGetTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}
