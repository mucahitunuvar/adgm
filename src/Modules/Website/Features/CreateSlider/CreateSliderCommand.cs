using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateSlider;

public sealed record CreateSliderCommand(string? Key, string? DefaultLanguageName) : IRequest<Result<CreateSliderResponse>>;
