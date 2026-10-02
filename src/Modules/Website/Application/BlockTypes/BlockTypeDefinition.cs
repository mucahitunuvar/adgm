using System.Text.Json;
using System.Text.Json.Serialization;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// §4.1: shared parse/validate/describe plumbing for every concrete block type - deserializing raw
// JSON strictly (UnmappedMemberHandling.Disallow: "bilinmeyen alanlar reddedilir ... tip uyuşmazlığı
// 400 döner"), re-serializing to canonical JSON for storage, and enforcing "the default language's
// texts are required" (§4.3). Concrete block types only implement the three things that differ per
// type: shape-only settings validation, cross-field settings/texts validation (per present language),
// and which entities the block references.
public abstract class BlockTypeDefinition<TSettings, TTexts> : IBlockTypeDefinition
    where TSettings : class
    where TTexts : class
{
    protected BlockTypeDefinition(IHtmlContentSanitizer? htmlContentSanitizer = null)
    {
        HtmlSanitizer = htmlContentSanitizer;
    }

    // Only `rich-text` and `image-text` need this (their Body field is sanitized HTML, §1 "Zengin
    // metin alanları handler'da IHtmlContentSanitizer ile temizlenir" - here, that "handler" is this
    // shared ParseAndValidate). Every other block type leaves its constructor's default (null) and
    // never calls it.
    protected IHtmlContentSanitizer? HtmlSanitizer { get; }

    private static readonly JsonSerializerOptions StrictOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    private static readonly JsonSerializerOptions CanonicalOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public abstract string Key { get; }

    public abstract IReadOnlyList<PageLayoutTargetKind> AllowedTargets { get; }

    public bool HasTexts => typeof(TTexts) != typeof(EmptyBlockTexts);

    public IReadOnlyList<BlockTypeFieldDescriptor> DescribeSettingsFields() =>
        BlockTypeFieldDescriber.Describe(typeof(TSettings), isLanguageDependent: false);

    public IReadOnlyList<BlockTypeFieldDescriptor> DescribeTextsFields() =>
        BlockTypeFieldDescriber.Describe(typeof(TTexts), isLanguageDependent: true);

    public Result<ParsedBlockContent> ParseAndValidate(
        JsonElement settingsElement, IReadOnlyDictionary<LanguageCode, JsonElement> textsByLanguage, LanguageCode defaultLanguageCode)
    {
        var settingsResult = Deserialize<TSettings>(settingsElement, $"{Key}.InvalidSettings");
        if (settingsResult.IsFailure)
        {
            return Result.Failure<ParsedBlockContent>(settingsResult.Error);
        }

        var settings = settingsResult.Value;
        var settingsValidation = ValidateSettings(settings);
        if (settingsValidation.IsFailure)
        {
            return Result.Failure<ParsedBlockContent>(settingsValidation.Error);
        }

        if (!HasTexts)
        {
            return Result.Success(new ParsedBlockContent(
                JsonSerializer.Serialize(settings, CanonicalOptions),
                new Dictionary<LanguageCode, string>(),
                GetReferences(settings, [])));
        }

        if (!textsByLanguage.ContainsKey(defaultLanguageCode))
        {
            return Result.Failure<ParsedBlockContent>(Error.Validation(
                $"{Key}.DefaultLanguageTextsRequired",
                $"A block of type '{Key}' must have texts in the default language '{defaultLanguageCode}'."));
        }

        var textsJsonByLanguage = new Dictionary<LanguageCode, string>();
        var parsedTexts = new List<TTexts>();

        foreach (var (languageCode, element) in textsByLanguage)
        {
            var textsResult = Deserialize<TTexts>(element, $"{Key}.InvalidTexts");
            if (textsResult.IsFailure)
            {
                return Result.Failure<ParsedBlockContent>(textsResult.Error);
            }

            var textsValidation = ValidateTexts(settings, textsResult.Value);
            if (textsValidation.IsFailure)
            {
                return Result.Failure<ParsedBlockContent>(textsValidation.Error);
            }

            var sanitizedTexts = SanitizeTexts(textsResult.Value);
            parsedTexts.Add(sanitizedTexts);
            textsJsonByLanguage[languageCode] = JsonSerializer.Serialize(sanitizedTexts, CanonicalOptions);
        }

        return Result.Success(new ParsedBlockContent(
            JsonSerializer.Serialize(settings, CanonicalOptions), textsJsonByLanguage, GetReferences(settings, parsedTexts)));
    }

    protected abstract Result ValidateSettings(TSettings settings);

    // Called once per language present in the request (always including the default language) - must
    // hold for every one of them, e.g. array-length parity between settings' items and this
    // language's texts' items (§4.2 "dizi uzunlukları ... eşleşmelidir").
    protected abstract Result ValidateTexts(TSettings settings, TTexts texts);

    // Runs once per language, after ValidateTexts succeeds and before the texts are re-serialized and
    // stored - the hook `rich-text`/`image-text` override to sanitize their Body field.
    protected virtual TTexts SanitizeTexts(TTexts texts) => texts;

    // texts holds every language's already-validated Texts record - needed because a handful of
    // block types (video-feature) carry a LinkTarget inside Texts rather than Settings (§4.2's table
    // puts "link" in the Metinler column for that one type).
    protected abstract BlockReferenceSet GetReferences(TSettings settings, IReadOnlyList<TTexts> texts);

    public BlockReferenceSet ExtractReferences(JsonElement settingsElement, IReadOnlyDictionary<LanguageCode, JsonElement> textsByLanguage)
    {
        TSettings? settings;
        try
        {
            settings = settingsElement.Deserialize<TSettings>(StrictOptions);
        }
        catch (JsonException)
        {
            return BlockReferenceSet.Empty;
        }

        if (settings is null)
        {
            return BlockReferenceSet.Empty;
        }

        var texts = new List<TTexts>();
        foreach (var element in textsByLanguage.Values)
        {
            try
            {
                if (element.Deserialize<TTexts>(StrictOptions) is { } text)
                {
                    texts.Add(text);
                }
            }
            catch (JsonException)
            {
            }
        }

        return GetReferences(settings, texts);
    }

    private Result<T> Deserialize<T>(JsonElement element, string errorCode)
    {
        try
        {
            var value = element.Deserialize<T>(StrictOptions);
            if (value is null)
            {
                return Result.Failure<T>(Error.Validation(errorCode, $"Block '{Key}' has invalid or missing content."));
            }

            return Result.Success(value);
        }
        catch (JsonException ex)
        {
            return Result.Failure<T>(Error.Validation(errorCode, $"Block '{Key}' has invalid content: {ex.Message}"));
        }
    }
}
