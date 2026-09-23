using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;

public sealed class FakeHostEnvironment : IHostEnvironment
{
    public string ContentRootPath { get; set; } = string.Empty;

    public string ApplicationName { get; set; } = "GenclikMerkezi.UnitTests";
    public string EnvironmentName { get; set; } = "Testing";
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
}
