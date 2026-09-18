using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.ExportCandidateCvPdf;

// CallerIsPrivileged is resolved by the endpoint (from the caller's role claims) rather than here -
// this handler only combines it with the resource-ownership check, it never reads roles itself
// (AGENTS.md §20: role/claim inspection is a transport-level concern, kept in Presentation).
public sealed record ExportCandidateCvPdfQuery(Guid CandidateCvId, bool CallerIsPrivileged)
    : IRequest<Result<CandidateCvPdfFile>>;
