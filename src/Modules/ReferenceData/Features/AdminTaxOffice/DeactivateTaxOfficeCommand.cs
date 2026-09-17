using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;

public sealed record DeactivateTaxOfficeCommand(Guid Id) : IRequest<Result>;
