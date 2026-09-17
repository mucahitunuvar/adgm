using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminLookupCrud;

public sealed record DeactivateLookupItemCommand<TLookup>(Guid Id)
    : IRequest<Result>
    where TLookup : LookupItem, ILookupItemFactory<TLookup>;
