using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetSitemapSegment;

public sealed record GetSitemapSegmentQuery(int Segment) : IRequest<Result<string>>;
