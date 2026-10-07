namespace GenclikMerkezi.SharedKernel.Text;

// Search-oriented normalization (originally ADR-020's CandidateSearchIndex.FullNameNormalized;
// moved here for ADR-024 §10's website-wide search, Faz 5 Görev 1 - Website must not depend on
// Candidate, ADR-024 §10 explicitly calls for reusing this function). Folds Turkish-specific letters
// to a single canonical form BEFORE calling ToUpperInvariant, so the classic "Turkish I" ambiguity
// never appears. ToUpperInvariant/ToLowerInvariant alone are not enough - .NET's invariant lowercase
// of 'İ' (U+0130) produces "i" + a combining dot (two chars), not a plain "i", and CultureInfo("tr-TR")
// casing would fold 'I' -> 'ı' instead of 'i'. Manually replacing the ambiguous letters first, then
// applying ToUpperInvariant to the (now Turkish-char-free) remainder, sidesteps both issues and keeps
// the result one-codepoint-per-letter.
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
