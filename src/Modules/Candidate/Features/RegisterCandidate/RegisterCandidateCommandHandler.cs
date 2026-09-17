using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;

// Kayıt orkestrasyonu (Candidate module design ADR, Decision 2): "adaya kayıt olma" iş akışı
// Candidate modülünün sorumluluğunda yaşar, Identity yalnızca hesap altyapısını sağlar. Identity'nin
// ve Candidate'ın ayrı veritabanları olduğu için (AGENTS.md §9) dağıtık transaction yok - adım 2
// başarısız olursa adım 1'de oluşturulan User, telafi (compensation) olarak Deactivate edilir.
public sealed class RegisterCandidateCommandHandler(
    IIdentityService identityService,
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterCandidateCommand, Result<RegisterCandidateResponse>>
{
    private const string CandidateRole = "Candidate";

    public async Task<Result<RegisterCandidateResponse>> Handle(
        RegisterCandidateCommand request, CancellationToken cancellationToken)
    {
        var createUserResult = await identityService.CreateUserAsync(
            request.Email, request.Password, request.FirstName, request.LastName, request.PhoneNumber, CandidateRole, cancellationToken);

        if (createUserResult.IsFailure)
        {
            return Result.Failure<RegisterCandidateResponse>(createUserResult.Error);
        }

        var userId = createUserResult.Value;

        try
        {
            var candidateCv = CandidateCv.Create(
                userId, request.FirstName, request.LastName, request.Email, request.PhoneNumber);
            candidateCvRepository.Add(candidateCv);

            var candidateCvContent = CandidateCvContent.Create(candidateCv.Id);
            candidateCvContentRepository.Add(candidateCvContent);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new RegisterCandidateResponse(userId, candidateCv.Id));
        }
        catch
        {
            // Compensating action: the User account was already committed by Identity's own
            // transaction. Rather than leave an orphaned account with no CandidateCv, deactivate it
            // and let the original failure surface (GlobalExceptionHandler maps it to a 500 -
            // this is an unexpected infrastructure failure, not an expected business-rule Result).
            await identityService.DeactivateUserAsync(userId, cancellationToken);
            throw;
        }
    }
}
