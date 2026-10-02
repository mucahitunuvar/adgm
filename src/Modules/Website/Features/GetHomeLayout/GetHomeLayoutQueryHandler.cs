using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetHomeLayout;

public sealed class GetHomeLayoutQueryHandler(IPageLayoutRepository pageLayoutRepository)
    : IRequestHandler<GetHomeLayoutQuery, Result<GetHomeLayoutResponse>>
{
    public async Task<Result<GetHomeLayoutResponse>> Handle(GetHomeLayoutQuery request, CancellationToken cancellationToken)
    {
        var layout = await pageLayoutRepository.GetHomeAsync(cancellationToken)
            ?? throw new InvalidOperationException("The home page layout is missing its seeded row.");

        return Result.Success(GetHomeLayoutResponse.FromDomain(layout));
    }
}
