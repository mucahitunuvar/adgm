using System.Text.Json;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// §4.3: shared by both ReplaceHomeDraftBlocksCommandHandler/ReplaceContentDraftBlocksCommandHandler
// (build fresh LayoutBlock entities from the admin's request) and PublishHomeLayoutCommandHandler/
// PublishContentLayoutCommandHandler ("bu anda referans doğrulaması tekrar yapılır" - re-checking the
// CURRENT draft's references at publish time, since ImpactMetric/ContentItem/ContentType references
// are not protected by a usage checker the way media/video/slider are, §4.3's own remarks, so one
// could have been deleted since the draft was last saved).
public sealed class LayoutBlockInputProcessor(IBlockTypeRegistry blockTypeRegistry, PageLayoutReferenceValidator referenceValidator)
{
    public async Task<Result<IReadOnlyList<LayoutBlock>>> ProcessAsync(
        IReadOnlyList<LayoutBlockInput> inputs,
        PageLayoutTargetKind targetKind,
        LanguageCode defaultLanguageCode,
        CancellationToken cancellationToken)
    {
        var blocks = new List<LayoutBlock>();
        var referenceSets = new List<BlockReferenceSet>();

        foreach (var input in inputs)
        {
            var definitionResult = ResolveDefinition(input.BlockTypeKey, targetKind);
            if (definitionResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<LayoutBlock>>(definitionResult.Error);
            }

            var textsByLanguageResult = ParseLanguageKeys(input.Texts ?? new Dictionary<string, JsonElement>());
            if (textsByLanguageResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<LayoutBlock>>(textsByLanguageResult.Error);
            }

            var parseResult = definitionResult.Value.ParseAndValidate(input.Settings, textsByLanguageResult.Value, defaultLanguageCode);
            if (parseResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<LayoutBlock>>(parseResult.Error);
            }

            referenceSets.Add(parseResult.Value.References);

            var translations = parseResult.Value.TextsJsonByLanguage
                .Select(kvp => LayoutBlockTranslation.Create(kvp.Key, kvp.Value))
                .ToList();

            var blockResult = LayoutBlock.Create(input.BlockTypeKey, input.SortOrder, input.IsActive, parseResult.Value.SettingsJson, translations);
            if (blockResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<LayoutBlock>>(blockResult.Error);
            }

            blocks.Add(blockResult.Value);
        }

        var referenceValidation = await referenceValidator.ValidateAsync(referenceSets, cancellationToken);
        if (referenceValidation.IsFailure)
        {
            return Result.Failure<IReadOnlyList<LayoutBlock>>(referenceValidation.Error);
        }

        return Result.Success<IReadOnlyList<LayoutBlock>>(blocks);
    }

    public async Task<Result> ValidateStoredBlocksAsync(
        IReadOnlyList<LayoutBlock> blocks, PageLayoutTargetKind targetKind, LanguageCode defaultLanguageCode, CancellationToken cancellationToken)
    {
        var referenceSets = new List<BlockReferenceSet>();

        foreach (var block in blocks)
        {
            var definitionResult = ResolveDefinition(block.BlockTypeKey, targetKind);
            if (definitionResult.IsFailure)
            {
                return Result.Failure(definitionResult.Error);
            }

            var settingsElement = JsonSerializer.Deserialize<JsonElement>(block.SettingsJson);
            var textsByLanguage = block.Translations.ToDictionary(
                t => t.LanguageCode, t => JsonSerializer.Deserialize<JsonElement>(t.TextsJson));

            var parseResult = definitionResult.Value.ParseAndValidate(settingsElement, textsByLanguage, defaultLanguageCode);
            if (parseResult.IsFailure)
            {
                return Result.Failure(parseResult.Error);
            }

            referenceSets.Add(parseResult.Value.References);
        }

        return await referenceValidator.ValidateAsync(referenceSets, cancellationToken);
    }

    private Result<IBlockTypeDefinition> ResolveDefinition(string blockTypeKey, PageLayoutTargetKind targetKind)
    {
        var definition = blockTypeRegistry.TryGet(blockTypeKey);
        if (definition is null)
        {
            return Result.Failure<IBlockTypeDefinition>(Error.Validation(
                "PageLayout.UnknownBlockType", $"Unknown block type '{blockTypeKey}'."));
        }

        if (!definition.AllowedTargets.Contains(targetKind))
        {
            return Result.Failure<IBlockTypeDefinition>(Error.Validation(
                "PageLayout.BlockTypeNotAllowedForTarget", $"Block type '{blockTypeKey}' cannot be used in a '{targetKind}' layout."));
        }

        return Result.Success(definition);
    }

    private static Result<IReadOnlyDictionary<LanguageCode, JsonElement>> ParseLanguageKeys(IReadOnlyDictionary<string, JsonElement> texts)
    {
        var result = new Dictionary<LanguageCode, JsonElement>();

        foreach (var (languageCodeValue, element) in texts)
        {
            var languageCodeResult = LanguageCode.Create(languageCodeValue);
            if (languageCodeResult.IsFailure)
            {
                return Result.Failure<IReadOnlyDictionary<LanguageCode, JsonElement>>(languageCodeResult.Error);
            }

            result[languageCodeResult.Value] = element;
        }

        return Result.Success<IReadOnlyDictionary<LanguageCode, JsonElement>>(result);
    }
}
