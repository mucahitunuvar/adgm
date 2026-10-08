namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §15 (Faz 5 Görev 6): which schema.org structured-data shape a ContentType's items get on
// the public detail response (BreadcrumbList is always added regardless of this value; Event is
// added automatically for SupportsEvent types independently of this value - see
// StructuredDataBuilder's remarks). None is the default - most seed types (Sayfa, Ekip, Belge,
// Etkinlik/Eğitim, Gönüllülük Fırsatı) carry no extra structured data beyond BreadcrumbList/Event.
public enum ContentSchemaKind
{
    None,
    Article,
    NewsArticle,
    FaqPage,
}
