using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.ClosePersonnelNeed;

// HTTP endpoint'i yok (master prompt item 7) - yalnızca IPersonnelNeedModuleContract.CloseAsync
// üzerinden (Matching modülü, Görev 7) tetiklenecek. closedByAdvisorId, çağıran modülün zaten
// doğruladığı bir kimliktir (CareerAdvisorModuleContract.CreateMeetingRequestAsync deseniyle aynı -
// contract sınırında tekrar doğrulanmaz).
public sealed record ClosePersonnelNeedCommand(
    Guid PersonnelNeedId, Guid ClosedByAdvisorId, Guid? FulfilledByCandidateCvId) : IRequest<Result>;
