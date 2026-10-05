using System.Text.Json;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.SubmitFormSubmission;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetFormSubmissionById;

public sealed class GetFormSubmissionByIdQueryHandler(
    IFormSubmissionRepository formSubmissionRepository, IFormDefinitionRepository formDefinitionRepository)
    : IRequestHandler<GetFormSubmissionByIdQuery, Result<FormSubmissionDetailResponse>>
{
    private static readonly Error NotFoundError = Error.NotFound("FormSubmission.NotFound", "This submission could not be found.");

    public async Task<Result<FormSubmissionDetailResponse>> Handle(GetFormSubmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var submission = await formSubmissionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (submission is null)
        {
            return Result.Failure<FormSubmissionDetailResponse>(NotFoundError);
        }

        var form = await formDefinitionRepository.GetByIdAsync(submission.FormDefinitionId, cancellationToken);
        if (form is null)
        {
            return Result.Failure<FormSubmissionDetailResponse>(NotFoundError);
        }

        var snapshot = JsonSerializer.Deserialize<List<FormSubmissionFieldSnapshot>>(submission.FieldDefinitionsSnapshotJson) ?? [];
        var responses = JsonSerializer.Deserialize<Dictionary<string, object?>>(submission.ResponsesJson) ?? [];

        var answers = snapshot
            .Select(field => new FormSubmissionAnswerResponse(
                field.Key, field.Label, field.Type, responses.GetValueOrDefault(field.Key)))
            .ToList();

        var files = submission.FileAttachments
            .Select(f => new FormSubmissionFileResponse(f.Id, f.FieldKey, f.File.OriginalFileName, f.File.ContentType, f.File.SizeInBytes))
            .ToList();

        var acceptedLegalVersions = submission.AcceptedLegalVersions
            .Select(a => new FormSubmissionAcceptedLegalVersionResponse(a.LegalDocumentKey.Value, a.VersionNumber, a.IsPrivacyNotice))
            .ToList();

        var statusHistory = submission.StatusHistory
            .OrderBy(h => h.ChangedAtUtc)
            .Select(h => new FormSubmissionStatusHistoryEntryResponse(h.FromStatus.ToString(), h.ToStatus.ToString(), h.ChangedByUserId, h.ChangedAtUtc))
            .ToList();

        var internalNotes = submission.InternalNotes
            .OrderBy(n => n.CreatedAtUtc)
            .Select(n => new FormSubmissionInternalNoteResponse(n.Id, n.AuthorUserId, n.Text, n.CreatedAtUtc))
            .ToList();

        var response = new FormSubmissionDetailResponse(
            submission.Id,
            submission.ReferenceNumber,
            form.Key.Value,
            submission.DefinitionVersion,
            submission.LanguageCode.Value,
            submission.SubmittedAtUtc,
            submission.SubmittedByUserId,
            submission.SourceContentItemId,
            submission.Status.ToString(),
            submission.AssignedToUserId,
            submission.ClosedAtUtc,
            submission.ArchivedAtUtc,
            submission.AnonymizedAtUtc,
            submission.RowVersion,
            answers,
            files,
            acceptedLegalVersions,
            statusHistory,
            internalNotes);

        return Result.Success(response);
    }
}
