using System.Text;

namespace GenclikMerkezi.Modules.Employer.Domain;

// Saf domain mantığı (AGENTS.md §10/§43: infra bağımlılığı yok). Website'in kendi Slug VO'suna
// (ASCII-katlama + id eki) kasıtlı olarak bağımlı değil - Employer, Website'e bağımlı olamaz
// (master prompt §1 "Website modülüne dokunulmaz").
public static class JobSlugGenerator
{
    private const int MaxTitleSlugLength = 60;
    private const int IdSuffixLength = 8;
    private const string EmptyTitleFallback = "ilan";

    public static string Generate(string title, Guid jobId)
    {
        var titleSlug = Slugify(title);
        var idSuffix = jobId.ToString("N")[..IdSuffixLength];

        return $"{titleSlug}-{idSuffix}";
    }

    private static string Slugify(string title)
    {
        var builder = new StringBuilder(title.Length);
        var previousWasHyphen = false;

        foreach (var ch in title)
        {
            var mapped = MapToAscii(ch);

            if (mapped is (>= 'a' and <= 'z') or (>= '0' and <= '9'))
            {
                builder.Append(mapped);
                previousWasHyphen = false;
            }
            else if (builder.Length > 0 && !previousWasHyphen)
            {
                builder.Append('-');
                previousWasHyphen = true;
            }
        }

        var slug = builder.ToString().Trim('-');

        if (slug.Length > MaxTitleSlugLength)
        {
            slug = slug[..MaxTitleSlugLength].Trim('-');
        }

        return slug.Length == 0 ? EmptyTitleFallback : slug;
    }

    // Türkçe İ/ı çifti, invariant ToLower ile birleştirici işaret üretir (İ -> "i" + U+0307);
    // bu yüzden Türkçe harfler, culture-bağımsız tek karakterlik hedeflere burada ayrıca eşlenir,
    // kalan her şey normal ToLowerInvariant ile küçültülür.
    private static char MapToAscii(char ch) => ch switch
    {
        'ş' or 'Ş' => 's',
        'ğ' or 'Ğ' => 'g',
        'ü' or 'Ü' => 'u',
        'ö' or 'Ö' => 'o',
        'ç' or 'Ç' => 'c',
        'ı' or 'İ' => 'i',
        _ => char.ToLowerInvariant(ch),
    };
}
