using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ReindexSearchSource;

public sealed record ReindexSearchSourceCommand(string SourceKey) : IRequest<Result>;
