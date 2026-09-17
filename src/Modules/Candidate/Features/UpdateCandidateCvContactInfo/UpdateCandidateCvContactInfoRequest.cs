namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvContactInfo;

public sealed record UpdateCandidateCvContactInfoRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    Guid? CountryId,
    Guid? ProvinceId,
    Guid? DistrictId,
    string? Address,
    IReadOnlyList<SocialMediaLinkRequest> SocialMediaLinks);
