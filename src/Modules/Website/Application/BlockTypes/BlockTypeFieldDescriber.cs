using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GenclikMerkezi.Modules.Website.Application.BlockTypes;

// §4.1 "Bu bilgi C# kayıtlarından tek kaynaktan üretilmeli; elle ikinci bir şema yazılmamalı."
//
// No reflection-over-a-DTO "describe my shape" utility exists anywhere else in this repo (checked
// before writing this), and AGENTS.md §47 asks that a NuGet package not be added merely to save a
// little hand-written code - so this is a small, self-contained System.Reflection walk over each
// block type's Settings/Texts record, not a JSON-Schema library. "Required" comes straight from C#
// nullability (a non-nullable reference/value-type property has no way to be absent), so no
// [Required] attribute is needed. "Limits" (string/array length, numeric range) use the standard
// System.ComponentModel.DataAnnotations attributes already in the BCL - read here purely as
// documentation metadata for this endpoint; they do not run as validation (FluentValidation/the
// block type's own ValidateSettings/ValidateTexts remain the sole enforcement), so there is no
// dual-source-of-truth risk - the attribute sits directly on the same property that is serialized,
// so the limit it describes cannot drift from the shape actually stored.
internal static class BlockTypeFieldDescriber
{
    public static IReadOnlyList<BlockTypeFieldDescriptor> Describe(Type recordType, bool isLanguageDependent)
    {
        if (recordType == typeof(EmptyBlockSettings) || recordType == typeof(EmptyBlockTexts))
        {
            return [];
        }

        var nullabilityContext = new NullabilityInfoContext();

        return recordType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => DescribeProperty(property, nullabilityContext, isLanguageDependent))
            .ToList();
    }

    private static BlockTypeFieldDescriptor DescribeProperty(
        PropertyInfo property, NullabilityInfoContext nullabilityContext, bool isLanguageDependent)
    {
        var propertyType = property.PropertyType;
        var isCollection = typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string);
        var elementType = isCollection ? propertyType.GetGenericArguments().FirstOrDefault() ?? typeof(object) : propertyType;
        var underlyingElementType = Nullable.GetUnderlyingType(elementType) ?? elementType;

        var isNullableValueType = Nullable.GetUnderlyingType(propertyType) is not null;
        var isNullableReferenceType = !propertyType.IsValueType
            && nullabilityContext.Create(property).WriteState == NullabilityState.Nullable;

        var minLength = property.GetCustomAttribute<MinLengthAttribute>()?.Length;
        var maxLength = property.GetCustomAttribute<MaxLengthAttribute>()?.Length;
        var range = property.GetCustomAttribute<RangeAttribute>();

        var nestedFields = IsDescribableRecord(underlyingElementType)
            ? Describe(underlyingElementType, isLanguageDependent)
            : null;

        return new BlockTypeFieldDescriptor(
            Name: ToCamelCase(property.Name),
            Type: DescribeTypeName(isCollection, underlyingElementType),
            IsRequired: !isNullableValueType && !isNullableReferenceType,
            IsLanguageDependent: isLanguageDependent,
            MinLength: minLength,
            MaxLength: maxLength,
            Min: range is null ? null : Convert.ToDouble(range.Minimum),
            Max: range is null ? null : Convert.ToDouble(range.Maximum),
            Fields: nestedFields);
    }

    // A nested item record (QuickLinkItemSettings, LinkTargetDto, ...) vs. a leaf scalar we already
    // describe by name (string/int/decimal/bool/Guid/DateTime).
    private static bool IsDescribableRecord(Type type) =>
        type.IsClass
        && type != typeof(string)
        && type != typeof(object);

    private static string DescribeTypeName(bool isCollection, Type underlyingElementType)
    {
        var elementName = underlyingElementType switch
        {
            _ when underlyingElementType == typeof(string) => "string",
            _ when underlyingElementType == typeof(int) => "int",
            _ when underlyingElementType == typeof(decimal) => "decimal",
            _ when underlyingElementType == typeof(bool) => "boolean",
            _ when underlyingElementType == typeof(Guid) => "guid",
            _ when underlyingElementType == typeof(DateTime) => "dateTime",
            _ => "object",
        };

        return isCollection ? $"array<{elementName}>" : elementName;
    }

    private static string ToCamelCase(string name) =>
        name.Length == 0 ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
