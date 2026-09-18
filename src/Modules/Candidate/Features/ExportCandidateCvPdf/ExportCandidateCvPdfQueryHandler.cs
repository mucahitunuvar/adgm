using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.ExportCandidateCvPdf;

public sealed class ExportCandidateCvPdfQueryHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    ICurrentUserContext currentUserContext,
    IFileStorageService fileStorageService,
    IReferenceDataLookupReader referenceDataLookupReader,
    ICandidateCvPdfExportService pdfExportService)
    : IRequestHandler<ExportCandidateCvPdfQuery, Result<CandidateCvPdfFile>>
{
    public async Task<Result<CandidateCvPdfFile>> Handle(ExportCandidateCvPdfQuery request, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(request.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return Result.Failure<CandidateCvPdfFile>(Error.NotFound("CandidateCv.NotFound", "The specified candidate CV could not be found."));
        }

        var isOwner = candidateCv.UserId == currentUserContext.UserId;

        if (!isOwner && !request.CallerIsPrivileged)
        {
            return Result.Failure<CandidateCvPdfFile>(
                Error.Forbidden("CandidateCv.NotOwner", "You may only export your own candidate CV."));
        }

        var candidateCvContent = await candidateCvContentRepository.GetByCandidateCvIdAsync(candidateCv.Id, cancellationToken);

        var photoBytes = candidateCv.Photo is not null
            ? await fileStorageService.ReadAsync(candidateCv.Photo.FileKey, cancellationToken)
            : null;

        var lookupNames = await ResolveLookupNamesAsync(
            CandidateCvPdfModelBuilder.CollectLookupIds(candidateCv, candidateCvContent), cancellationToken);

        var model = CandidateCvPdfModelBuilder.Build(candidateCv, candidateCvContent, photoBytes, lookupNames);
        var pdfBytes = pdfExportService.Generate(model);
        var fileName = $"{candidateCv.FirstName}_{candidateCv.LastName}_CV.pdf";

        return Result.Success(new CandidateCvPdfFile(pdfBytes, fileName));
    }

    private async Task<IReadOnlyDictionary<(ReferenceDataLookupType Type, Guid Id), string>> ResolveLookupNamesAsync(
        IReadOnlyDictionary<ReferenceDataLookupType, HashSet<Guid>> idsByType, CancellationToken cancellationToken)
    {
        var lookups = await Task.WhenAll(idsByType.Select(async kvp =>
        {
            var items = await referenceDataLookupReader.GetByIdsAsync(kvp.Key, kvp.Value, cancellationToken);
            return (Type: kvp.Key, Items: items);
        }));

        var result = new Dictionary<(ReferenceDataLookupType, Guid), string>();

        foreach (var (type, items) in lookups)
        {
            foreach (var item in items)
            {
                result[(type, item.Id)] = item.DisplayName;
            }
        }

        return result;
    }
}
