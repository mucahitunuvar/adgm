using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteNewsletterSubscriber;

// ADR-024 §14 (Faz 3 Görev 6): "kişinin silme talebi için" - an admin-initiated hard delete on request
// of the data subject, distinct from the retention job's own automatic cleanup. No RowVersion: deleting
// a whole row has nothing left to race against once it is gone, unlike every other mutator on this
// aggregate.
public sealed class DeleteNewsletterSubscriberCommandHandler(
    INewsletterSubscriberRepository newsletterSubscriberRepository,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteNewsletterSubscriberCommand, Result>
{
    public async Task<Result> Handle(DeleteNewsletterSubscriberCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await newsletterSubscriberRepository.GetByIdAsync(request.Id, cancellationToken);
        if (subscriber is null)
        {
            return Result.Failure(Error.NotFound("NewsletterSubscriber.NotFound", "This subscriber could not be found."));
        }

        newsletterSubscriberRepository.Remove(subscriber);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
