namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.5 (Faz 1b Görev 6): the suffix texts DuplicateContentItemCommandHandler appends when
// copying a content item - defined in exactly one place ("Sonek metinleri tek bir yerde tanımlanır").
// The slug suffix word is chosen once, from the site's default language, and reused for every
// translation's slug; the title suffix is chosen per translation, from that translation's own language.
public static class ContentItemDuplicateSuffixes
{
    public static string SlugSuffixWord(LanguageCode defaultLanguageCode) =>
        defaultLanguageCode.Value == "tr" ? "kopya" : "copy";

    public static string TitleSuffix(LanguageCode languageCode) =>
        languageCode.Value == "tr" ? " (Kopya)" : " (Copy)";
}
