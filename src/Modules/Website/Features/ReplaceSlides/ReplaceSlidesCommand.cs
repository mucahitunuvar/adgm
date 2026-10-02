using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceSlides;

public sealed record ReplaceSlidesCommand(Guid SliderId, byte[] RowVersion, IReadOnlyList<SlideInput> Slides) : IRequest<Result>;
