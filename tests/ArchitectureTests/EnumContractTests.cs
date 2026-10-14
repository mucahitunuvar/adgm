using System.Text.Json;
using ManifestModules = System.Collections.Generic.SortedDictionary<string, System.Collections.Generic.SortedDictionary<string, System.Collections.Generic.SortedDictionary<string, int>>>;

namespace GenclikMerkezi.ArchitectureTests;

// Faz 1 bulgusu (docs/prompts/FRONTEND-OPENAPI-PRODUCES-PROMPT.md'nin devamı): Identity ve Website
// modüllerindeki enum'ların çoğu yanıtlarda `.ToString()` ile elle string'e çevrilir (DTO alanı
// `string`); OpenAPI bu yüzden yalnız "type: string" görür, olası değerlerin listesini göremez.
// Kablo biçimini (JSON'da string mi sayı mı gittiğini) DEĞİŞTİRMEDEN bu enum'ların kod sözleşmesini
// (ad + sayısal değer) makineyle okunabilir, sapma-korumalı bir envanterde (docs/api/enums.v1.json)
// yayınlıyoruz. Bir üye yeniden adlandırılır/silinir/yeniden numaralanırsa veya yeni bir enum
// eklenirse bu test düşer - route+method anlık görüntü testiyle aynı mantık (Görev 0'daki
// karşılaştırma).
public class EnumContractTests
{
    private static readonly string[] TargetModules = ["Identity", "Website"];

    [Fact]
    public void Enum_Contract_Matches_Committed_Manifest()
    {
        var current = BuildCurrentManifest();
        var manifestPath = Path.Combine(AppContext.BaseDirectory, "TestData", "enums.v1.json");
        var committedJson = File.ReadAllText(manifestPath);
        var committed = JsonSerializer.Deserialize<ManifestModules>(committedJson)
            ?? throw new InvalidOperationException("docs/api/enums.v1.json okunamadı veya boş.");

        var differences = Compare(committed, current);

        Assert.True(
            differences.Count == 0,
            "Enum sözleşmesi docs/api/enums.v1.json ile uyuşmuyor (bir üye eklenmiş/silinmiş/yeniden " +
            "numaralanmış veya yeni bir enum var). Bu kasıtlıysa docs/api/enums.v1.json'u güncelleyin:" +
            Environment.NewLine + string.Join(Environment.NewLine, differences));
    }

    // İki ayrı yerden çağrılır: bu test (denetim) ve manifest'i ilk kez üreten tek seferlik araç.
    // Aynı mantığı paylaşmaları, "nasıl üretildi" ile "nasıl denetlendi" arasında sapma olmamasını
    // garanti eder.
    internal static ManifestModules BuildCurrentManifest()
    {
        var modules = new ManifestModules(StringComparer.Ordinal);

        foreach (var assembly in ModuleAssemblies.All)
        {
            var moduleName = assembly.GetName().Name!.Split('.').Last();
            if (!TargetModules.Contains(moduleName))
            {
                continue;
            }

            var domainNamespace = $"{assembly.GetName().Name}.Domain";
            var enums = new SortedDictionary<string, SortedDictionary<string, int>>(StringComparer.Ordinal);

            foreach (var type in assembly.SafeGetTypes())
            {
                if (!type.IsEnum || type.Namespace != domainNamespace || !type.IsPublic)
                {
                    continue;
                }

                var members = new SortedDictionary<string, int>(StringComparer.Ordinal);
                foreach (var name in Enum.GetNames(type))
                {
                    members[name] = Convert.ToInt32(Enum.Parse(type, name));
                }

                enums[type.Name] = members;
            }

            modules[moduleName] = enums;
        }

        return modules;
    }

    private static List<string> Compare(ManifestModules committed, ManifestModules current)
    {
        var differences = new List<string>();

        foreach (var moduleName in TargetModules)
        {
            var committedEnums = committed.GetValueOrDefault(moduleName) ?? new(StringComparer.Ordinal);
            var currentEnums = current.GetValueOrDefault(moduleName) ?? new(StringComparer.Ordinal);

            foreach (var enumName in committedEnums.Keys.Union(currentEnums.Keys, StringComparer.Ordinal))
            {
                var hasCommitted = committedEnums.TryGetValue(enumName, out var committedMembers);
                var hasCurrent = currentEnums.TryGetValue(enumName, out var currentMembers);

                if (!hasCommitted)
                {
                    differences.Add($"{moduleName}.{enumName}: manifest'te yok, kodda var (yeni enum).");
                    continue;
                }

                if (!hasCurrent)
                {
                    differences.Add($"{moduleName}.{enumName}: manifest'te var, kodda yok (silinmiş).");
                    continue;
                }

                foreach (var memberName in committedMembers!.Keys.Union(currentMembers!.Keys, StringComparer.Ordinal))
                {
                    var hasCommittedMember = committedMembers.TryGetValue(memberName, out var committedValue);
                    var hasCurrentMember = currentMembers.TryGetValue(memberName, out var currentValue);

                    if (!hasCommittedMember)
                    {
                        differences.Add($"{moduleName}.{enumName}.{memberName}: manifest'te yok (yeni üye, değer {currentValue}).");
                    }
                    else if (!hasCurrentMember)
                    {
                        differences.Add($"{moduleName}.{enumName}.{memberName}: kodda yok (silinmiş üye).");
                    }
                    else if (committedValue != currentValue)
                    {
                        differences.Add($"{moduleName}.{enumName}.{memberName}: değer {committedValue} -> {currentValue}.");
                    }
                }
            }
        }

        return differences;
    }
}
