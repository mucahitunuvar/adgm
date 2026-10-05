using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetFormDefinitions;

public sealed class GetFormDefinitionsQueryHandler(IFormDefinitionRepository formDefinitionRepository, ISiteLanguageRepository siteLanguageRepository)
    : IRequestHandler<GetFormDefinitionsQuery, Result<IReadOnlyList<FormDefinitionSummaryResponse>>>
{
    public async Task<Result<IReadOnlyList<FormDefinitionSummaryResponse>>> Handle(
        GetFormDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<IReadOnlyList<FormDefinitionSummaryResponse>>(
                Error.Failure("FormDefinition.NoDefaultLanguage", "No default site language is configured."));
        }

        var forms = await formDefinitionRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<FormDefinitionSummaryResponse> responses = forms
            .Select(f => new FormDefinitionSummaryResponse(
                f.Id, f.Key.Value, ResolveTitle(f, defaultLanguage.Code), f.IsActive, f.DefinitionVersion, f.Fields.Count, f.RowVersion))
            .ToList();

        return Result.Success(responses);
    }

    private static string ResolveTitle(FormDefinition form, LanguageCode defaultLanguageCode) =>
        form.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguageCode)?.Title ?? form.Key.Value;
}
