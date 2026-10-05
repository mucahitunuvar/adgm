using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class FormSubmissionRepository(WebsiteDbContext dbContext) : IFormSubmissionRepository
{
    public Task<FormSubmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.FormSubmissions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public void Add(FormSubmission formSubmission) => dbContext.FormSubmissions.Add(formSubmission);
}
