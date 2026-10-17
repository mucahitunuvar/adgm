using System.Globalization;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.Http;

// Minimal API's built-in binding for a nullable value-type query parameter (Guid?, int?, bool?,
// DateTime?) throws BadHttpRequestException on ANY value it cannot parse - including an empty string
// ("?provinceId="), which is indistinguishable at that point from "?provinceId=not-a-guid". The
// global exception handler then turns that into a 500 (confirmed live: GET /public/jobs?provinceId=
// and ?provinceId=abc both 500). An endpoint that wants "empty = filter not applied, malformed = 400"
// (every optional query filter on a public list endpoint) must bind the parameter as plain `string?`
// and parse it through this helper instead of letting the framework bind it directly.
public static class OptionalQueryParameter
{
    public static Result<T?> Parse<T>(string? rawValue, string parameterName) where T : struct, IParsable<T>
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return Result.Success<T?>(null);
        }

        if (T.TryParse(rawValue, CultureInfo.InvariantCulture, out var parsed))
        {
            return Result.Success<T?>(parsed);
        }

        var capitalizedParameterName = char.ToUpperInvariant(parameterName[0]) + parameterName[1..];
        return Result.Failure<T?>(Error.Validation(
            $"Query.Invalid{capitalizedParameterName}", $"'{parameterName}' query parameter is not a valid {typeof(T).Name} value."));
    }
}
