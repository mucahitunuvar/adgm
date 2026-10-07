using System.Text;
using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.Modules.Website.Features.ExportEventRegistrations;

// ADR-024 §11.2 (Faz 4 Görev 5): same shape as NewsletterSubscriberCsvFormatter (Faz 3 Görev 6) - a
// leading UTF-8 BOM (U+FEFF, so Excel reads ş/ğ/ı/İ/ö/ü/ç correctly) and a single-quote prefix on any
// cell starting with '=', '+', '-' or '@' (CSV/formula injection).
public static class EventRegistrationCsvFormatter
{
    private static readonly char[] FormulaPrefixes = ['=', '+', '-', '@'];

    public static byte[] Format(IReadOnlyList<EventRegistrationExportItem> items)
    {
        var builder = new StringBuilder();
        builder.Append('﻿');
        builder.Append("firstName,lastName,email,phone,status,registeredAtUtc,confirmedAtUtc,language\r\n");

        foreach (var item in items)
        {
            builder.Append(EscapeCell(item.FirstName));
            builder.Append(',');
            builder.Append(EscapeCell(item.LastName));
            builder.Append(',');
            builder.Append(EscapeCell(item.Email));
            builder.Append(',');
            builder.Append(EscapeCell(item.Phone ?? string.Empty));
            builder.Append(',');
            builder.Append(EscapeCell(item.Status.ToString()));
            builder.Append(',');
            builder.Append(EscapeCell(item.RegisteredAtUtc.ToString("O")));
            builder.Append(',');
            builder.Append(EscapeCell(item.ConfirmedAtUtc?.ToString("O") ?? string.Empty));
            builder.Append(',');
            builder.Append(EscapeCell(item.LanguageCode));
            builder.Append("\r\n");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string EscapeCell(string value)
    {
        var escaped = value.Length > 0 && FormulaPrefixes.Contains(value[0]) ? "'" + value : value;

        return escaped.Contains(',') || escaped.Contains('"') || escaped.Contains('\n')
            ? "\"" + escaped.Replace("\"", "\"\"") + "\""
            : escaped;
    }
}
