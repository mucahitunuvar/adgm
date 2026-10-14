namespace GenclikMerkezi.SharedKernel.Parsing;

// Enum.TryParse accepts two kinds of input that are NOT a real member name, and callers across
// Identity/Website were (before this fix) relying on its bare bool result without catching either:
// 1. A numeric string ("999") parses into that raw underlying value even when no member has it.
// 2. A comma-separated list of names ("Draft,Published") parses by OR-ing the members' underlying
//    values together, even for an enum that is not [Flags] - and when that OR'd value happens to
//    equal a different, unrelated member's value, it silently becomes THAT member instead of being
//    rejected. Enum.IsDefined does not catch this second case (the OR'd result can itself be a
//    defined value) - verified live: AdminGetUsers?status=Pending,Active (0|1=1) returns the same
//    12 rows as ?status=Active.
// TryParseDefined closes both by requiring an exact (trimmed, optionally case-insensitive) match
// against one of the enum's actual declared names - the numeric and OR-combination parsing paths
// inside Enum.TryParse are never reached.
public static class EnumParsing
{
    public static bool TryParseDefined<TEnum>(string? value, out TEnum result, bool ignoreCase = true)
        where TEnum : struct, Enum
    {
        if (value is not null)
        {
            var trimmed = value.Trim();
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            foreach (var name in Enum.GetNames<TEnum>())
            {
                if (string.Equals(name, trimmed, comparison))
                {
                    result = Enum.Parse<TEnum>(name);
                    return true;
                }
            }
        }

        result = default;
        return false;
    }
}
