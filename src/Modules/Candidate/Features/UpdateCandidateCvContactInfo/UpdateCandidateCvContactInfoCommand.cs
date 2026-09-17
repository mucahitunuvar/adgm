using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvContactInfo;

public sealed record UpdateCandidateCvContactInfoCommand(
    Guid CandidateCvId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    Guid? CountryId,
    Guid? ProvinceId,
    Guid? DistrictId,
    string? Address,
    IReadOnlyList<SocialMediaLinkRequest> SocialMediaLinks) : IRequest<Result>;
