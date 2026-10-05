using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.SetFormFields;

public sealed record SetFormFieldsCommand(Guid Id, byte[] RowVersion, IReadOnlyList<FormFieldInput> Fields) : IRequest<Result>;
