namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4 (Faz 5 Görev 7): why this particular revision was saved. Created is the very first
// revision (initial save or the first revision of a duplicated copy); Edited is any later text/SEO/
// tag/category change while not the live published snapshot; Published is the exact snapshot that
// went live (IsPublishedSnapshot is only ever true together with this kind); Restored is the revision
// produced by rolling a single language back to an earlier snapshot.
public enum ContentItemRevisionKind
{
    Created,
    Edited,
    Published,
    Restored,
}
