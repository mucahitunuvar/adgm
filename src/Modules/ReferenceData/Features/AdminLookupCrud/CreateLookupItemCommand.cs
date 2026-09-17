using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminLookupCrud;

public sealed record CreateLookupItemCommand<TLookup>(string Code, string DisplayName, int SortOrder)
    : IRequest<Result<Guid>>
    where TLookup : LookupItem, ILookupItemFactory<TLookup>;
