using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.SendBulkNotification;

// Endpoint'siz komut (Identity'nin CreateStaffUserCommand deseni) - yalnızca
// ICareerAdvisorModuleContract.SendBulkNotificationAsync üzerinden, Candidate modülünün
// SendBulkCandidateNotificationCommand'ından ISender ile çağrılır (Görev 8). candidateUserIds,
// çağıran tarafından zaten sahiplik doğrulaması yapılmış bir liste olarak gelir.
public sealed record SendBulkNotificationCommand(IReadOnlyList<Guid> CandidateUserIds, string Subject, string Message)
    : IRequest<Result>;
