using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.RecalculateCompletionPercentage;

public sealed class CandidateCvContentUpdatedRecalculatesCompletionPercentageHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
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

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
