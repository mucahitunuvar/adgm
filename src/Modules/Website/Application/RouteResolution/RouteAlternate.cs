namespace GenclikMerkezi.Modules.Website.Application.RouteResolution;

// ADR-024 §15 (Faz 1a Görev 6): a resolved item's or listing's public path in another active
// language (hreflang). Application-layer shape - the Features layer maps it to its own HTTP response
// record rather than reusing this one, keeping the vertical slice's response contract independent of
// this internal shape.
public sealed record RouteAlternate(string LanguageCode, string Path);
