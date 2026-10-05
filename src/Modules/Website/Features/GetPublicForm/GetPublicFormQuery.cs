using GenclikMerkezi.Modules.Website.Application.Forms.PublicResolution;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicForm;

public sealed record GetPublicFormQuery(string Key, string? Lang) : IRequest<Result<PublicFormDefinitionResponse>>;
