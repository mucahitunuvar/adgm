using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.ClosePersonnelNeed;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Employer.Infrastructure;

// CareerAdvisorModuleContract deseniyle aynı: GetGeneralPoolAsync saf bir okuma projeksiyonu olduğu
// için doğrudan DbContext'e sorgu atıyor (CompanyModuleContract deseni), CloseAsync ise gerçek iş
// mantığı içerdiği için kendi modülünün iç MediatR komutuna delege ediyor
// (CreateMeetingRequestAsync deseni).
public sealed class PersonnelNeedModuleContract(EmployerDbContext dbContext, ISender sender) : IPersonnelNeedModuleContract
{
    public Task<PagedResult<PersonnelNeedSummary>> GetGeneralPoolAsync(
        PagedRequest request, CancellationToken cancellationToken = default)
    {
        return dbContext.PersonnelNeeds
            .AsNoTracking()
            .Where(p => p.Status == PersonnelNeedStatus.GenelHavuzda)
            .OrderByDescending(p => p.PooledAtUtc)
            .Select(p => new PersonnelNeedSummary(
                p.Id, p.CompanyId, p.EmploymentTypeId, p.WorkLocationTypeId, p.PositionId, p.DepartmentId,
                p.Quantity, p.ProvinceId, p.ExperienceLevelId, p.DetailsText, p.PooledByAdvisorId, p.PooledAtUtc))
            .ToPagedResultAsync(request, cancellationToken);
    }

    public Task<Result> CloseAsync(
        Guid personnelNeedId, Guid closedByAdvisorId, Guid? fulfilledByCandidateCvId, CancellationToken cancellationToken = default)
    {
        return sender.Send(new ClosePersonnelNeedCommand(personnelNeedId, closedByAdvisorId, fulfilledByCandidateCvId), cancellationToken);
    }
}
