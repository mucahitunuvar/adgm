using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;

// §4.2 "job-list bloğu yalnızca ayar tutar; veriyi frontend Employer API'sinden çeker" and
// "job-list yalnızca Home hedefinde kullanılabilir" - Home only, and no entity references at all
// since Website never stores job/listing data itself (§1 "Employer'daki eksikler ayrı bir işte
// yapılacak").
public sealed class JobListBlockTypeDefinition : BlockTypeDefinition<JobListBlockSettings, JobListBlockTexts>
{
    public override string Key => "job-list";

    public override IReadOnlyList<PageLayoutTargetKind> AllowedTargets => [PageLayoutTargetKind.Home];

    protected override Result ValidateSettings(JobListBlockSettings settings)
    {
        if (settings.Count is < 1 or > 12)
        {
            return Result.Failure(Error.Validation("job-list.CountInvalid", "Count must be between 1 and 12."));
        }

        return Result.Success();
    }

    protected override Result ValidateTexts(JobListBlockSettings settings, JobListBlockTexts texts)
    {
        if (string.IsNullOrWhiteSpace(texts.Title))
        {
            return Result.Failure(Error.Validation("job-list.TitleRequired", "A title is required."));
        }

        return Result.Success();
    }

    protected override BlockReferenceSet GetReferences(JobListBlockSettings settings, IReadOnlyList<JobListBlockTexts> texts) =>
        BlockReferenceSet.Empty;
}
