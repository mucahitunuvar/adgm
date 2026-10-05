using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class FormSubmissionRepository(WebsiteDbContext dbContext) : IFormSubmissionRepository
{
    public Task<FormSubmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.FormSubmissions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public void Add(FormSubmission formSubmission) => dbContext.FormSubmissions.Add(formSubmission);

    public async Task<PagedResult<FormSubmissionListItem>> SearchAsync(
        FormSubmissionSearchFilter filter, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        // An unparsable FormKey filter can never match a real FormDefinitionKey - short-circuit to an
        // empty page rather than translating a failed parse into a query (FormDefinitionKey's value
        // converter has no member-access translation for arbitrary string comparison, only whole-value
        // equality, the same constraint FormDefinitionRepository.GetByKeyAsync relies on).
        FormDefinitionKey? formKey = null;
        if (!string.IsNullOrWhiteSpace(filter.FormKey))
        {
            var formKeyResult = FormDefinitionKey.Create(filter.FormKey);
            if (formKeyResult.IsFailure)
            {
                return new PagedResult<FormSubmissionListItem>([], 0, pagedRequest.Page, pagedRequest.PageSize);
            }

            formKey = formKeyResult.Value;
        }

        FormSubmissionStatus? status = null;
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            if (!Enum.TryParse<FormSubmissionStatus>(filter.Status, out var parsedStatus))
            {
                return new PagedResult<FormSubmissionListItem>([], 0, pagedRequest.Page, pagedRequest.PageSize);
            }

            status = parsedStatus;
        }

        var query =
            from submission in dbContext.FormSubmissions.AsNoTracking()
            join form in dbContext.FormDefinitions.AsNoTracking() on submission.FormDefinitionId equals form.Id
            where filter.Archived ? submission.ArchivedAtUtc != null : submission.ArchivedAtUtc == null
            select new { submission, form };

        if (formKey is not null)
        {
            query = query.Where(x => x.form.Key == formKey);
        }

        if (status is not null)
        {
            query = query.Where(x => x.submission.Status == status.Value);
        }

        if (filter.AssignedToUserId is not null)
        {
            query = query.Where(x => x.submission.AssignedToUserId == filter.AssignedToUserId.Value);
        }

        if (filter.FromUtc is not null)
        {
            query = query.Where(x => x.submission.SubmittedAtUtc >= filter.FromUtc.Value);
        }

        if (filter.ToUtc is not null)
        {
            query = query.Where(x => x.submission.SubmittedAtUtc <= filter.ToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.ReferenceNumber))
        {
            query = query.Where(x => x.submission.ReferenceNumber.Contains(filter.ReferenceNumber));
        }

        var projected = query
            .OrderByDescending(x => x.submission.SubmittedAtUtc)
            .Select(x => new FormSubmissionListItem(
                x.submission.Id,
                x.submission.ReferenceNumber,
                x.form.Key,
                x.submission.Status.ToString(),
                x.submission.SubmittedAtUtc,
                x.submission.AssignedToUserId,
                x.submission.FileAttachments.Count,
                x.submission.ArchivedAtUtc != null,
                x.submission.RowVersion));

        return await projected.ToPagedResultAsync(pagedRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<FormSubmission>> GetDueForArchiveAsync(
        DateTime eligibleBeforeUtc, CancellationToken cancellationToken = default) =>
        await dbContext.FormSubmissions
            .Where(s => s.ArchivedAtUtc == null && s.ArchiveEligibleSinceUtc != null && s.ArchiveEligibleSinceUtc <= eligibleBeforeUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<FormSubmission>> GetDueForAnonymizationAsync(
        DateTime nowUtc, int maxCount, CancellationToken cancellationToken = default)
    {
        var candidateIds = await (
                from submission in dbContext.FormSubmissions.AsNoTracking()
                join form in dbContext.FormDefinitions.AsNoTracking() on submission.FormDefinitionId equals form.Id
                where submission.AnonymizedAtUtc == null && submission.SubmittedAtUtc.AddDays(form.RetentionDays) <= nowUtc
                select submission.Id)
            .Take(maxCount)
            .ToListAsync(cancellationToken);

        if (candidateIds.Count == 0)
        {
            return [];
        }

        return await dbContext.FormSubmissions.Where(s => candidateIds.Contains(s.Id)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FormSubmission>> GetWithPendingFileDeletionsAsync(
        int maxCount, CancellationToken cancellationToken = default)
    {
        var candidateIds = await dbContext.FormSubmissions
            .AsNoTracking()
            .Where(s => s.PendingFileDeletions.Count > 0)
            .Select(s => s.Id)
            .Take(maxCount)
            .ToListAsync(cancellationToken);

        if (candidateIds.Count == 0)
        {
            return [];
        }

        return await dbContext.FormSubmissions.Where(s => candidateIds.Contains(s.Id)).ToListAsync(cancellationToken);
    }
}
