using System.Globalization;
using System.Text;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §11/§17 (Faz 4 Görev 2): a single public VEVENT per event schedule - no attendee data, no
// OnlineLink (§1 "Online link ve kişisel veri yoktur"). Pure text assembly; the caller resolves every
// input (title, location, absolute page URL, UID domain) from already-loaded data.
public static class IcsEventCalendarBuilder
{
    private const int MaxLineOctets = 75;

    public static string Build(
        Guid contentItemId,
        string uidDomain,
        string title,
        string descriptionSummary,
        string eventPageUrl,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        string location,
        bool isCancelled,
        DateTime generatedAtUtc)
    {
        var lines = new List<string>
        {
            "BEGIN:VCALENDAR",
            "VERSION:2.0",
            "PRODID:-//GenclikMerkezi//Website//TR",
            "CALSCALE:GREGORIAN",
            "BEGIN:VEVENT",
            $"UID:{contentItemId}@{uidDomain}",
            $"DTSTAMP:{FormatUtc(generatedAtUtc)}",
            $"DTSTART:{FormatUtc(startsAtUtc)}",
            $"DTEND:{FormatUtc(endsAtUtc)}",
            $"SUMMARY:{Escape(title)}",
            $"LOCATION:{Escape(location)}",
            $"DESCRIPTION:{Escape(BuildDescription(descriptionSummary, eventPageUrl))}",
            $"URL:{Escape(eventPageUrl)}",
        };

        if (isCancelled)
        {
            lines.Add("STATUS:CANCELLED");
        }

        lines.Add("END:VEVENT");
        lines.Add("END:VCALENDAR");

        var builder = new StringBuilder();
        foreach (var line in lines)
        {
            builder.Append(FoldLine(line));
            builder.Append("\r\n");
        }

        return builder.ToString();
    }

    private static string BuildDescription(string summary, string eventPageUrl) =>
        string.IsNullOrWhiteSpace(summary) ? eventPageUrl : $"{summary}\n{eventPageUrl}";

    // Every caller-supplied value is already UTC by field naming contract (StartsAtUtc, EndsAtUtc,
    // generatedAtUtc) - formatted as-is rather than via ToUniversalTime(), which would wrongly treat
    // a DateTimeKind.Unspecified value (e.g. a SQLite-backed EventSchedule read back without a Kind
    // tag) as local time and shift it by the machine's offset.
    private static string FormatUtc(DateTime value) => value.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);

    // RFC 5545 §3.3.11 TEXT escaping - backslash first, so the characters it introduces for the other
    // three rules are never themselves re-escaped.
    private static string Escape(string value) =>
        value
            .Replace("\\", "\\\\")
            .Replace(";", "\\;")
            .Replace(",", "\\,")
            .Replace("\r\n", "\n")
            .Replace("\n", "\\n");

    // RFC 5545 §3.1 line folding: at or before 75 octets (UTF-8 bytes, never splitting a multi-byte
    // character) per physical line, each continuation prefixed by a single space.
    private static string FoldLine(string line)
    {
        if (Encoding.UTF8.GetByteCount(line) <= MaxLineOctets)
        {
            return line;
        }

        var builder = new StringBuilder();
        var currentLineOctets = 0;
        foreach (var ch in line)
        {
            var charOctets = Encoding.UTF8.GetByteCount(ch.ToString());
            if (currentLineOctets + charOctets > MaxLineOctets)
            {
                builder.Append("\r\n ");
                currentLineOctets = 1;
            }

            builder.Append(ch);
            currentLineOctets += charOctets;
        }

        return builder.ToString();
    }
}
