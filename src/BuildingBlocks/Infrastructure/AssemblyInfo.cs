using System.Runtime.CompilerServices;

// Lets MemoryCacheServiceTests assert on MemoryCacheService.TrackedKeyCount (internal) - the only
// black-box-observable proof that ADR-017's PostEvictionCallback wiring actually shrinks the key
// tracking set on natural TTL expiry, not just on an explicit Remove/RemoveByPrefix.
[assembly: InternalsVisibleTo("GenclikMerkezi.UnitTests")]
