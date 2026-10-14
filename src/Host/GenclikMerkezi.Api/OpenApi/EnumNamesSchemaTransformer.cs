using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace GenclikMerkezi.Api.OpenApi;

// Kablo biçimini (JSON'da enum'ların sayı mı string mi gittiğini) DEĞİŞTİRMEZ - yalnız OpenAPI
// şemasını, enum üyelerinin adlarını taşıyan bir "x-enum-varnames" uzantısıyla zenginleştirir.
// Bu, numaralı (JsonStringEnumConverter'sız) bir enum alanı için OpenAPI'nin "enum: [0,1,2]" gibi
// yalnız sayı gösterdiği, hangi sayının hangi isme karşılık geldiğinin belgelenmediği durumu
// çözer - Domain'e veya herhangi bir Response/Handler'a dokunmadan (bkz. Faz 1 bulgusu).
public sealed class EnumNamesSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        var enumType = Nullable.GetUnderlyingType(type) ?? type;

        if (!enumType.IsEnum)
        {
            return Task.CompletedTask;
        }

        var names = Enum.GetNames(enumType);
        var varNames = new JsonArray();
        foreach (var name in names)
        {
            varNames.Add(JsonValue.Create(name));
        }

        schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        schema.Extensions["x-enum-varnames"] = new JsonNodeExtension(varNames);

        return Task.CompletedTask;
    }
}
