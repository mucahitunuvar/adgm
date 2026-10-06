using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetThirdPartyScriptById;

public sealed class GetThirdPartyScriptByIdQueryHandler(IThirdPartyScriptRepository thirdPartyScriptRepository)
    : IRequestHandler<GetThirdPartyScriptByIdQuery, Result<ThirdPartyScriptDetailResponse>>
{
    public async Task<Result<ThirdPartyScriptDetailResponse>> Handle(GetThirdPartyScriptByIdQuery request, CancellationToken cancellationToken)
    {
        var script = await thirdPartyScriptRepository.GetByIdAsync(request.Id, cancellationToken);
        if (script is null)
        {
            return Result.Failure<ThirdPartyScriptDetailResponse>(
                Error.NotFound("ThirdPartyScript.NotFound", $"Third-party script '{request.Id}' could not be found."));
        }

        var translations = script.Translations
            .Select(t => new ThirdPartyScriptTranslationResponse(t.LanguageCode.Value, t.Name, t.Purpose))
            .ToList();

        var response = new ThirdPartyScriptDetailResponse(
            script.Id, script.Provider.Kind.ToString(), script.Provider.MeasurementId, script.Provider.ContainerId, script.Provider.PixelId,
            script.Provider.Src, script.Provider.Async, script.Provider.Defer, script.Category.ToString(), script.Placement.ToString(),
            script.SortOrder, script.IsActive, script.RowVersion, translations, script.CreatedAtUtc);

        return Result.Success(response);
    }
}
