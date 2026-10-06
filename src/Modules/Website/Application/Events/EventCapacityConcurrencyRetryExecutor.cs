using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Application.Events;

// ADR-024 §11.2 (Faz 4 Görev 3): "EventSchedule.RowVersion çakışmasında en fazla 3 kez yeniden
// denenir, sonra 409" - the one place that retry loop is implemented, shared by
// CreateEventRegistrationCommandHandler (capacity decision), VerifyEventRegistrationCommandHandler
// (capacity decision) and CancelEventRegistrationCommandHandler (capacity release), so the retry/
// reload mechanics are written once instead of three times.
//
// `mutate` is re-invoked on every attempt, including retries - it must be safe to call more than
// once against the same in-memory EventSchedule/EventRegistration instances (ReserveCapacity/
// ReleaseConfirmedSlot/ReleaseWaitlistSlot are; EventRegistration.ApplyCapacityDecision is, by design -
// see its own remarks). SaveChangesAsync is the only EF Core-specific call this makes on behalf of
// the caller; catching DbUpdateConcurrencyException here (rather than behind a SharedKernel-level
// abstraction) is a deliberate, narrow choice for this one call site - see the Görev 3 completion
// report for why.
public sealed class EventCapacityConcurrencyRetryExecutor(
    IEventScheduleRepository eventScheduleRepository,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
{
    public const int MaxAttempts = 4;

    private static readonly Error ConflictError = Error.Conflict(
        "Event.RegistrationConflict", "Could not complete the operation due to a concurrent update. Please try again.");

    public async Task<Result> ExecuteAsync(EventSchedule eventSchedule, Func<Result> mutate, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var mutateResult = mutate();
            if (mutateResult.IsFailure)
            {
                return mutateResult;
            }

            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (attempt >= MaxAttempts)
                {
                    break;
                }

                await eventScheduleRepository.ReloadAsync(eventSchedule, cancellationToken);
            }
        }

        return Result.Failure(ConflictError);
    }
}
