using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.RecordPersonalDataAccess;

// ADR-024 §12.2/§14 (Faz 3 Görev 5): "Yazma işlemi okuma isteğinin parçası olarak ayrı bir komutla
// yapılır (Faz 1a'daki 404 kaydı deseni: sorgu yazmaz)" - called by each personal-data-bearing
// admin endpoint (GetFormSubmissionById, DownloadFormSubmissionFile, and Görev 6's newsletter list/
// export) only after its own read query already succeeded, the same best-effort, never-fails-the-
// response shape ResolveRouteEndpoint already uses for RecordNotFoundPathCommand.
public sealed class RecordPersonalDataAccessCommandHandler(
    IPersonalDataAccessLogRepository personalDataAccessLogRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RecordPersonalDataAccessCommand, Result>
{
    public async Task<Result> Handle(RecordPersonalDataAccessCommand request, CancellationToken cancellationToken)
    {
        var createResult = PersonalDataAccessLog.Create(
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime, request.EntityType, request.EntityId, request.Action,
            request.Detail);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        personalDataAccessLogRepository.Add(createResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
