using System.Text;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Features.ExportNewsletterSubscribers;

namespace GenclikMerkezi.UnitTests.Website.Features.ExportNewsletterSubscribers;

public class NewsletterSubscriberCsvFormatterTests
{
    [Fact]
    public void Format_StartsWithUtf8ByteOrderMark()
    {
        var bytes = NewsletterSubscriberCsvFormatter.Format([]);

        Assert.Equal([0xEF, 0xBB, 0xBF], bytes.Take(3));
    }

    [Fact]
    public void Format_WritesHeaderRow()
    {
        var bytes = NewsletterSubscriberCsvFormatter.Format([]);
        var text = Encoding.UTF8.GetString(bytes);

        Assert.StartsWith("﻿email,language,confirmedAtUtc\r\n", text);
    }

    [Fact]
    public void Format_WritesOneRowPerSubscriber()
    {
        var confirmedAt = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var items = new[] { new NewsletterSubscriberExportItem("test@example.com", "tr", confirmedAt) };

        var text = Encoding.UTF8.GetString(NewsletterSubscriberCsvFormatter.Format(items));

        Assert.Contains($"test@example.com,tr,{confirmedAt:O}\r\n", text);
    }

    [Fact]
    public void Format_ForPendingSubscriberWithNoConfirmedAt_LeavesCellEmpty()
    {
        var items = new[] { new NewsletterSubscriberExportItem("pending@example.com", "tr", null) };

        var text = Encoding.UTF8.GetString(NewsletterSubscriberCsvFormatter.Format(items));

        Assert.Contains("pending@example.com,tr,\r\n", text);
    }

    // ADR-024 §14: "CSV enjeksiyonuna karşı '=', '+', '-', '@' ile başlayan hücreler kaçışlanır" - the
    // email's local part can legitimately start with one of these characters (the format regex only
    // forbids '@' and whitespace), so a spreadsheet must never be allowed to treat it as a formula.
    [Theory]
    [InlineData("=cmd@example.com")]
    [InlineData("+1234@example.com")]
    [InlineData("-drop@example.com")]
    [InlineData("@malicious@example.com")]
    public void Format_EscapesCellsThatStartWithAFormulaCharacter(string dangerousEmail)
    {
        var items = new[] { new NewsletterSubscriberExportItem(dangerousEmail, "tr", null) };

        var text = Encoding.UTF8.GetString(NewsletterSubscriberCsvFormatter.Format(items));

        Assert.Contains($"'{dangerousEmail},tr,\r\n", text);
    }

    [Fact]
    public void Format_QuotesCellsContainingACommaOrQuote()
    {
        var items = new[] { new NewsletterSubscriberExportItem("weird@example.com", "tr,\"x\"", null) };

        var text = Encoding.UTF8.GetString(NewsletterSubscriberCsvFormatter.Format(items));

        Assert.Contains("\"tr,\"\"x\"\"\"", text);
    }
}
