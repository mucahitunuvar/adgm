using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;

public sealed record CreateTaxOfficeCommand(string Code, string DisplayName, int SortOrder, Guid ProvinceId)
    : IRequest<Result<Guid>>;
