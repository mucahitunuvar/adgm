using System.Text;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

// ADR-024 §11/§17 (Faz 4 Görev 2): format-level coverage for the public .ics export - RFC 5545 line
// folding/escaping/CRLF, stable UID, and STATUS:CANCELLED only when cancelled.
public class IcsEventCalendarBuilderTests
{
    private static readonly Guid ContentItemId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime StartsAtUtc = new(2026, 10, 10, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime EndsAtUtc = new(2026, 10, 10, 20, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime GeneratedAtUtc = new(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc);

    private static string Build(string title = "Başlık", string location = "Salon", bool isCancelled = false) =>
        IcsEventCalendarBuilder.Build(
            ContentItemId, "example.com", title, "Kısa özet", "https://example.com/etkinlikler/ornek", StartsAtUtc, EndsAtUtc, location,
            isCancelled, GeneratedAtUtc);

    [Fact]
    public void Build_UsesCrLfLineEndings()
    {
        var ics = Build();

        Assert.Contains("\r\n", ics);
        Assert.DoesNotContain("\n", ics.Replace("\r\n", string.Empty));
    }

    [Fact]
    public void Build_UidIsStableAndDerivedFromContentItemIdAndDomain()
    {
        var first = Build();
        var second = Build();

        Assert.Equal(first, second);
        Assert.Contains($"UID:{ContentItemId}@example.com\r\n", first);
    }

    [Fact]
    public void Build_FormatsStartAndEndAsUtcDateTime()
    {
        var ics = Build();

        Assert.Contains("DTSTART:20261010T180000Z\r\n", ics);
        Assert.Contains("DTEND:20261010T200000Z\r\n", ics);
    }

    [Fact]
    public void Build_WhenCancelled_IncludesStatusCancelled()
    {
        var ics = Build(isCancelled: true);

        Assert.Contains("STATUS:CANCELLED\r\n", ics);
    }

    [Fact]
    public void Build_WhenNotCancelled_OmitsStatus()
    {
        var ics = Build(isCancelled: false);

        Assert.DoesNotContain("STATUS", ics);
    }

    [Fact]
    public void Build_EscapesCommasSemicolonsAndBackslashesInText()
    {
        var ics = Build(title: "Konser; Açılış, \"Özel\" \\ Gösteri");

        Assert.Contains("SUMMARY:Konser\\; Açılış\\, \"Özel\" \\\\ Gösteri\r\n", ics);
    }

    [Fact]
    public void Build_LongLine_IsFoldedAtSeventyFiveOctetsWithLeadingSpaceContinuation()
    {
        var longTitle = new string('a', 200);
        var ics = Build(title: longTitle);

        var summaryLineStart = ics.IndexOf("SUMMARY:", StringComparison.Ordinal);
        var nextCrLf = ics.IndexOf("\r\n", summaryLineStart, StringComparison.Ordinal);
        var firstPhysicalLine = ics[summaryLineStart..nextCrLf];

        Assert.True(Encoding.UTF8.GetByteCount(firstPhysicalLine) <= 75);
        Assert.Contains("\r\n ", ics);

        // Folding must be reversible: stripping every CRLF+space continuation marker reconstructs the
        // original unfolded content line.
        var unfolded = ics.Replace("\r\n ", string.Empty);
        Assert.Contains($"SUMMARY:{longTitle}", unfolded);
    }
}
