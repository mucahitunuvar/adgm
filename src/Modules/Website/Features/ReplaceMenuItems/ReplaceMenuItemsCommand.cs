using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ReplaceMenuItems;

public sealed record ReplaceMenuItemsCommand(string Location, byte[] RowVersion, IReadOnlyList<MenuItemTreeInput> Items) : IRequest<Result>;
