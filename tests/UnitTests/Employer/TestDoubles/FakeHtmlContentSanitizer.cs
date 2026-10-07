using GenclikMerkezi.Modules.Employer.Application.Abstractions;

namespace GenclikMerkezi.UnitTests.Employer.TestDoubles;

public sealed class FakeHtmlContentSanitizer : IHtmlContentSanitizer
{
    public List<string> SanitizeCalledWith { get; } = [];

    public string Sanitize(string html)
    {
        SanitizeCalledWith.Add(html);
        return html;
    }
}
