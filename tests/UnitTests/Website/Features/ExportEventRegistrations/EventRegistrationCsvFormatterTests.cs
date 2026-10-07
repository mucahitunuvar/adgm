using System.Text;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.ExportEventRegistrations;

namespace GenclikMerkezi.UnitTests.Website.Features.ExportEventRegistrations;

public class EventRegistrationCsvFormatterTests
{
    private static readonly DateTime RegisteredAt = new(2026, 1, 10, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ConfirmedAt = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Format_StartsWithUtf8ByteOrderMark()
    {
        var bytes = EventRegistrationCsvFormatter.Format([]);

        Assert.Equal([0xEF, 0xBB, 0xBF], bytes.Take(3));
    }

    [Fact]
    public void Format_WritesHeaderRow()
    {
        var bytes = EventRegistrationCsvFormatter.Format([]);
        var text = Encoding.UTF8.GetString(bytes);

        Assert.StartsWith("﻿firstName,lastName,email,phone,status,registeredAtUtc,confirmedAtUtc,language\r\n", text);
    }

    [Fact]
    public void Format_WritesOneRowPerRegistration()
    {
        var items = new[]
        {
            new EventRegistrationExportItem(
                "Ahmet", "Yılmaz", "ahmet@example.com", "5551234567", EventRegistrationStatus.Confirmed, RegisteredAt, ConfirmedAt, "tr"),
        };

        var text = Encoding.UTF8.GetString(EventRegistrationCsvFormatter.Format(items));

        Assert.Contains($"Ahmet,Yılmaz,ahmet@example.com,5551234567,Confirmed,{RegisteredAt:O},{ConfirmedAt:O},tr\r\n", text);
    }

    [Fact]
    public void Format_ForRegistrationWithNoPhoneOrConfirmedAt_LeavesCellsEmpty()
    {
        var items = new[]
        {
            new EventRegistrationExportItem(
                "Ahmet", "Yılmaz", "ahmet@example.com", null, EventRegistrationStatus.Applied, RegisteredAt, null, "tr"),
        };

        var text = Encoding.UTF8.GetString(EventRegistrationCsvFormatter.Format(items));

        Assert.Contains($"Ahmet,Yılmaz,ahmet@example.com,,Applied,{RegisteredAt:O},,tr\r\n", text);
    }

    // §1 "CSV enjeksiyonuna karşı '=', '+', '-', '@' ile başlayan hücreler kaçışlanır".
    [Theory]
    [InlineData("=cmd")]
    [InlineData("+1234")]
    [InlineData("-drop")]
    [InlineData("@malicious")]
    public void Format_EscapesCellsThatStartWithAFormulaCharacter(string dangerousFirstName)
    {
        var items = new[]
        {
            new EventRegistrationExportItem(
                dangerousFirstName, "Yılmaz", "ahmet@example.com", null, EventRegistrationStatus.Applied, RegisteredAt, null, "tr"),
        };

        var text = Encoding.UTF8.GetString(EventRegistrationCsvFormatter.Format(items));

        Assert.Contains($"'{dangerousFirstName},Yılmaz", text);
    }

    [Fact]
    public void Format_QuotesCellsContainingACommaOrQuote()
    {
        var items = new[]
        {
            new EventRegistrationExportItem(
                "Ahmet", "Yılmaz, \"Lakap\"", "ahmet@example.com", null, EventRegistrationStatus.Applied, RegisteredAt, null, "tr"),
        };

        var text = Encoding.UTF8.GetString(EventRegistrationCsvFormatter.Format(items));

        Assert.Contains("\"Yılmaz, \"\"Lakap\"\"\"", text);
    }
}
