using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;

public sealed record CreateLegalDocumentCommand(
    string? Key, string? Kind, string? DefaultLanguageTitle) : IRequest<Result<CreateLegalDocumentResponse>>;
