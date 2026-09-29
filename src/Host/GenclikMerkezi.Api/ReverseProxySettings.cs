namespace GenclikMerkezi.Api;

// ADR-024 Faz 1b Görev 1: canlı ortam (Turhost/IIS) reverse proxy'siz çalışır, bu yüzden bu bayrak
// varsayılan olarak kapalıdır - HttpContext.Connection.RemoteIpAddress zaten gerçek ziyaretçi IP'sidir.
// İleride bir proxy/CDN (ör. Cloudflare) eklenirse Enabled=true yapılır ve yalnızca burada listelenen
// adreslerden/ağlardan gelen X-Forwarded-For kabul edilir - aksi halde herkes kendi IP'sini
// sahteleyebilir. Bu yüzden Enabled=true iken liste boş bırakılamaz (Program.cs başlangıçta doğrular).
public sealed class ReverseProxySettings
{
    public const string SectionName = "ReverseProxy";

    public bool Enabled { get; init; }

    public IReadOnlyList<string> KnownProxies { get; init; } = [];

    public IReadOnlyList<string> KnownNetworks { get; init; } = [];
}
