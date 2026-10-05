using System.Text;
using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.Modules.Website.Features.ExportNewsletterSubscribers;

// ADR-024 §14 (Faz 3 Görev 6): "CSV enjeksiyonuna karşı '=', '+', '-', '@' ile başlayan hücreler
// kaçışlanır" - prefixing with a single quote keeps a spreadsheet app from treating the cell as a
// formula while leaving the value itself readable. The leading U+FEFF is the UTF-8 byte order mark
// ("UTF-8 BOM'lu (Excel'de Türkçe karakterler için)") - without it, Excel guesses a legacy codepage and
// mangles ş/ğ/ı/İ/ö/ü/ç.
public static class NewsletterSubscriberCsvFormatter
{
    private static readonly char[] FormulaPrefixes = ['=', '+', '-', '@'];

    public static byte[] Format(IReadOnlyList<NewsletterSubscriberExportItem> items)
    {
        var builder = new StringBuilder();
        builder.Append('﻿');
        builder.Append("email,language,confirmedAtUtc\r\n");

        foreach (var item in items)
        {
            builder.Append(EscapeCell(item.Email));
            builder.Append(',');
            builder.Append(EscapeCell(item.LanguageCode));
            builder.Append(',');
            builder.Append(EscapeCell(item.ConfirmedAtUtc?.ToString("O") ?? string.Empty));
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
