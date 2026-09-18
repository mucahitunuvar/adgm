using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.SyncCandidateReadModels;

// Recalculates CompletionPercentage (Görev 6/ADR-018) and refreshes CandidateSearchIndex (ADR-020)
// from a single load of CandidateCv/CandidateCvContent and a single SaveChangesAsync - both derived
// read-models depend on the same source data, so combining them here avoids a second round trip a
// separate handler for the same event would cost, and keeps them updated in the same
// transaction/işlem akışı per ADR-020.
public sealed class CandidateCvUpdatedSyncsReadModelsHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    ICandidateSearchIndexRepository candidateSearchIndexRepository,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : INotificationHandler<DomainEventNotification<CandidateCvUpdatedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<CandidateCvUpdatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(notification.DomainEvent.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return;
        }

        var candidateCvContent = await candidateCvContentRepository.GetByCandidateCvIdAsync(candidateCv.Id, cancellationToken);

        var percentage = CandidateCvCompletionCalculator.Calculate(candidateCv, candidateCvContent);
        candidateCv.UpdateCompletionPercentage(percentage);

        var updatedAtUtc = DateTime.UtcNow;
        var searchIndex = await candidateSearchIndexRepository.GetByCandidateCvIdAsync(candidateCv.Id, cancellationToken);

        if (searchIndex is null)
        {
            // Defensive fallback - every CandidateCv should already have a row seeded at registration
            // (CandidateSearchIndexProjector.CreateInitial), but this keeps the read-model self-healing
            // rather than silently missing the candidate from listings if that ever isn't the case.
            searchIndex = CandidateSearchIndexProjector.CreateInitial(candidateCv, updatedAtUtc);
            candidateSearchIndexRepository.Add(searchIndex);
        }

        CandidateSearchIndexProjector.Project(searchIndex, candidateCv, candidateCvContent, updatedAtUtc);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
