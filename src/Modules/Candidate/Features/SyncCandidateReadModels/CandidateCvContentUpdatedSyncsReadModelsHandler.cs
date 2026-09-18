using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.SyncCandidateReadModels;

// See CandidateCvUpdatedSyncsReadModelsHandler for why CompletionPercentage recalculation and
// CandidateSearchIndex refresh are combined in one handler per event.
public sealed class CandidateCvContentUpdatedSyncsReadModelsHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    ICandidateSearchIndexRepository candidateSearchIndexRepository,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : INotificationHandler<DomainEventNotification<CandidateCvContentUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<CandidateCvContentUpdatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(notification.DomainEvent.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return;
        }

        var candidateCvContent = await candidateCvContentRepository.GetByIdAsync(
            notification.DomainEvent.CandidateCvContentId, cancellationToken);

        var percentage = CandidateCvCompletionCalculator.Calculate(candidateCv, candidateCvContent);
        candidateCv.UpdateCompletionPercentage(percentage);

        var updatedAtUtc = DateTime.UtcNow;
        var searchIndex = await candidateSearchIndexRepository.GetByCandidateCvIdAsync(candidateCv.Id, cancellationToken);

        if (searchIndex is null)
        {
            searchIndex = CandidateSearchIndexProjector.CreateInitial(candidateCv, updatedAtUtc);
            candidateSearchIndexRepository.Add(searchIndex);
        }

        CandidateSearchIndexProjector.Project(searchIndex, candidateCv, candidateCvContent, updatedAtUtc);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
