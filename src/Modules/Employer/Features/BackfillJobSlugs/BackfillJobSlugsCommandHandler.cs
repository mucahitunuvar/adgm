using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.BackfillJobSlugs;

// Görev 1 (master prompt): Slug alanı eklenmeden önce yayınlanmış ilanlara tek seferlik, idempotent
// geri doldurma - SQL'de değil, Approve() ile aynı JobSlugGenerator'ı kullanan Job.BackfillSlug() ile
// (SQLite ve SQL Server uyumlu). Zaten slug'ı olan ilanlar GetNeedingSlugBackfillAsync'in filtresine
// hiç girmez, bu yüzden tekrar çalıştırmak güvenlidir.
public sealed class BackfillJobSlugsCommandHandler(
    IJobRepository jobRepository,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<BackfillJobSlugsCommand, Result<BackfillJobSlugsResponse>>
{
    public async Task<Result<BackfillJobSlugsResponse>> Handle(
        BackfillJobSlugsCommand request, CancellationToken cancellationToken)
    {
        var jobs = await jobRepository.GetNeedingSlugBackfillAsync(cancellationToken);

        foreach (var job in jobs)
        {
            job.BackfillSlug();
        }

        if (jobs.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new BackfillJobSlugsResponse(jobs.Count));
    }
}
