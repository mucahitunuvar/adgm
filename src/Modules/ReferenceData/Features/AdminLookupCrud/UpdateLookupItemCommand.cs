using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminLookupCrud;

public sealed record UpdateLookupItemCommand<TLookup>(Guid Id, string DisplayName, int SortOrder)
    : IRequest<Result>
    where TLookup : LookupItem, ILookupItemFactory<TLookup>;
