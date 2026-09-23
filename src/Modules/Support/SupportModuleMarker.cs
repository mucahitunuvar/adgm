namespace GenclikMerkezi.Modules.Support;

public sealed class SupportModuleMarker
{
    // Diğer modüllerdeki UnitOfWorkKey deseniyle aynı gerekçe: her modül kendi IUnitOfWork'ünü bu
    // anahtarla çözer, aksi halde tek bir composition root'ta paylaşılan (unkeyed) bir kayıt
    // modüller arasında çakışırdı.
    public const string UnitOfWorkKey = "Support";
}
