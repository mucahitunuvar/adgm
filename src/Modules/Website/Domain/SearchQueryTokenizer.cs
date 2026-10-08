using System.Text;
using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Text;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §10 (Faz 5 Görev 3): turns the public search `q` into the token-AND LIKE patterns matched
// against SearchDocument.NormalizedText/Title. Reuses TurkishTextNormalizer (ADR-020) so matching is
// diacritic/case-insensitive, same as the indexing side (SearchTextBuilder.BuildNormalizedText). Each
// token is escaped against SQL LIKE's own wildcard characters before being wrapped in '%...%' -
// otherwise a query such as "50%" or "a_b" would silently match unrelated rows ("LIKE enjeksiyonu").
public static partial class SearchQueryTokenizer
{
    public const int MaxTokenCount = 6;
    public const string LikeEscapeCharacter = "\\";

    public static IReadOnlyList<string> BuildLikePatterns(string query)
    {
        var normalized = TurkishTextNormalizer.Normalize(query);
        var tokens = WhitespacePattern().Split(normalized).Where(t => t.Length > 0);

        return tokens
            .Take(MaxTokenCount)
            .Select(token => $"%{EscapeLikeToken(token)}%")
            .ToList();
    }

    private static string EscapeLikeToken(string token)
    {
        var escapeChar = LikeEscapeCharacter[0];
        var builder = new StringBuilder(token.Length);
        foreach (var ch in token)
        {
            if (ch is '%' or '_' or '[' || ch == escapeChar)
            {
                builder.Append(escapeChar);
            }

            builder.Append(ch);
        }

        return builder.ToString();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespacePattern();
}
