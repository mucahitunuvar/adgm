using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.UploadCompanyLogo;

public sealed record UploadCompanyLogoCommand(Guid CompanyId, Stream Content, string FileName, string ContentType)
    : IRequest<Result<string>>;
