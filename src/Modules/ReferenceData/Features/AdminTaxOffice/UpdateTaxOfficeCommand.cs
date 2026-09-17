using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;

// ProvinceId is intentionally not editable here - a tax office moving to a different province is
// a new record, not an edit of this one (same reasoning as any other lookup's Code being fixed at
// creation time).
public sealed record UpdateTaxOfficeCommand(Guid Id, string DisplayName, int SortOrder) : IRequest<Result>;
