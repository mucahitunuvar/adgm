using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class FormSubmissionSequenceRepository(WebsiteDbContext dbContext) : IFormSubmissionSequenceRepository
{
    public Task<FormSubmissionSequence?> GetByYearAsync(int year, CancellationToken cancellationToken = default) =>
        dbContext.FormSubmissionSequences.FirstOrDefaultAsync(s => s.Year == year, cancellationToken);

    public void Add(FormSubmissionSequence sequence) => dbContext.FormSubmissionSequences.Add(sequence);

    public void DetachFailedReservation(FormSubmissionSequence sequence) => dbContext.Entry(sequence).State = EntityState.Detached;
}
