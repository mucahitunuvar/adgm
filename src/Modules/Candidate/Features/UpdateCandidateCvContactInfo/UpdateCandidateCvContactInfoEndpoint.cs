using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvContactInfo;

internal static class UpdateCandidateCvContactInfoEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/api/v1/candidates/{candidateCvId:guid}/contact-info",
                async (
                    Guid candidateCvId,
                    UpdateCandidateCvContactInfoRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateCandidateCvContactInfoCommand(
                        candidateCvId,
                        request.FirstName,
                        request.LastName,
                        request.Email,
                        request.PhoneNumber,
                        request.CountryId,
                        request.ProvinceId,
                        request.DistrictId,
                        request.Address,
                        request.SocialMediaLinks);
                    var result = await sender.Send(command, cancellationToken);

                    return result.ToNoContentOrProblem();
                })
            .RequireAuthorization()
            .RequireRateLimiting("authenticated")
            .WithName("UpdateCandidateCvContactInfo")
            .WithTags("Candidate");
    }
}
