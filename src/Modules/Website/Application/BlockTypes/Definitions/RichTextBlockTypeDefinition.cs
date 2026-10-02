using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

// §4.2: the one block type with no language-independent settings at all ("Ayarlar: —").
public sealed class RichTextBlockTypeDefinition(IHtmlContentSanitizer htmlContentSanitizer)
    : BlockTypeDefinition<EmptyBlockSettings, RichTextBlockTexts>(htmlContentSanitizer)
{
    public override string Key => "rich-text";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home, PageLayoutTargetKind.Content];

    protected override Result ValidateSettings(EmptyBlockSettings settings) => Result.Success();

    protected override Result ValidateTexts(EmptyBlockSettings settings, RichTextBlockTexts texts)
    {
        if (string.IsNullOrWhiteSpace(texts.Body))
        {
            return Result.Failure(Error.Validation("rich-text.BodyRequired", "A body is required."));
        }

        return Result.Success();
    }

    protected override RichTextBlockTexts SanitizeTexts(RichTextBlockTexts texts) =>
        texts with { Body = HtmlSanitizer!.Sanitize(texts.Body) };

    protected override BlockReferenceSet GetReferences(EmptyBlockSettings settings, IReadOnlyList<RichTextBlockTexts> texts) =>
        BlockReferenceSet.Empty;
}
