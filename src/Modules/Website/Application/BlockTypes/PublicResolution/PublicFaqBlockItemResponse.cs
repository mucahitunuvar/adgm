namespace GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;

// Faz 2 Görev 5 master prompt §5.2: faq's per-item `data` - "başlık ve gövde" only, no image or path.
public sealed record PublicFaqBlockItemResponse(Guid Id, string Title, string Body);
