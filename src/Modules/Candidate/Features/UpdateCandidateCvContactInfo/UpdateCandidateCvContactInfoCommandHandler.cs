using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvContactInfo;

public sealed class UpdateCandidateCvContactInfoCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCandidateCvContactInfoCommand, Result>
{
    public async Task<Result> Handle(UpdateCandidateCvContactInfoCommand request, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(request.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return Result.Failure(Error.NotFound("CandidateCv.NotFound", "The specified candidate CV could not be found."));
        }

        if (candidateCv.UserId != currentUserContext.UserId)
        {
            return Result.Failure(Error.Forbidden("CandidateCv.NotOwner", "You may only update your own candidate CV."));
        }

        candidateCv.UpdateContactInfo(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.CountryId,
            request.ProvinceId,
            request.DistrictId,
            request.Address);

        // Replace-all semantics: simplest contract for a client sending "here is the current full
        // list" rather than individual add/remove operations, which Candidate.md does not call for
        // on social media links the way it does for Experience/Education/etc.
        foreach (var existingLink in candidateCv.SocialMediaLinks.ToList())
        {
            candidateCv.RemoveSocialMediaLink(existingLink.Id);
        }

        foreach (var link in request.SocialMediaLinks)
        {
            candidateCv.AddSocialMediaLink(link.Platform, link.Url);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
