namespace GenclikMerkezi.Modules.Website.Features.CompareContentItemRevisions;

// ADR-024 §4 (Faz 5 Görev 7): "her alan için changed bayrağı ve iki değer (metin fark algoritması
// yok, frontend gösterir)" - Changed is a plain equality check between From and To, nothing smarter.
public sealed record ContentItemRevisionFieldDiffResponse<T>(bool Changed, T From, T To);
