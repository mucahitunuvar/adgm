namespace GenclikMerkezi.Modules.Candidate.Domain;

// Search-oriented normalization for CandidateSearchIndex.FullNameNormalized (ADR-020): folds
// Turkish-specific letters to a single canonical form BEFORE calling ToUpperInvariant, so the
// classic "Turkish I" ambiguity never appears. ToUpperInvariant/ToLowerInvariant alone are not
// enough - .NET's invariant lowercase of 'İ' (U+0130) produces "i" + a combining dot (two chars),
// not a plain "i", and CultureInfo("tr-TR") casing would fold 'I' -> 'ı' instead of 'i'. Manually
// replacing the ambiguous letters first, then applying ToUpperInvariant to the (now Turkish-char-
// free) remainder, sidesteps both issues and keeps the result one-codepoint-per-letter.
public static class TurkishTextNormalizer
{
    public static string Normalize(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var folded = value
            .Replace('İ', 'I')
            .Replace('ı', 'I')
            .Replace('i', 'I')
            .Replace('Ş', 'S')
            .Replace('ş', 'S')
            .Replace('Ğ', 'G')
            .Replace('ğ', 'G')
            .Replace('Ü', 'U')
            .Replace('ü', 'U')
            .Replace('Ö', 'O')
            .Replace('ö', 'O')
            .Replace('Ç', 'C')
            .Replace('ç', 'C');

        return folded.ToUpperInvariant().Trim();
    }
}
