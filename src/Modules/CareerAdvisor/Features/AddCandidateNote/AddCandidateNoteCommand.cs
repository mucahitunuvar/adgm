using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerAdvisor.Features.AddCandidateNote;

// CandidateUserId açık bir parametre (Candidate modülünden CareerAdvisor'a doğrudan bir çağrı
// olmadan - ADR-022 §1 - bildirim için gereken profil bilgisine Identity'nin public contract'ı
// üzerinden ulaşılabilmesi için). İstemci bunu zaten elinde bulunduruyor: not eklerken önce
// GET /api/v1/candidates/{id} ile adayın profilini görüntülemiş olur, o response UserId'yi içerir.
// NoteType, RegisterUserCommand.Role ile aynı sebeple string: System.Text.Json varsayılan olarak
// enum'ları sayı olarak (de)serialize eder (bu projede JsonStringEnumConverter hiç yapılandırılmamış),
// bu yüzden transport'ta string taşınıp Handler'da Enum.Parse ile domain enum'a çevriliyor.
public sealed record AddCandidateNoteCommand(Guid CandidateCvId, Guid CandidateUserId, string NoteType, string Content)
    : IRequest<Result<AddCandidateNoteResponse>>;
