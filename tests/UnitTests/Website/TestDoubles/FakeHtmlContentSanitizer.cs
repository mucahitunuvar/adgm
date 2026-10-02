using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeHtmlContentSanitizer : IHtmlContentSanitizer
{
    public string Sanitize(string html) => html;
}
