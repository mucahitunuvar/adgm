using GenclikMerkezi.SharedKernel.Results;
using Microsoft.AspNetCore.Http;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.Http;

public static class ResultHttpExtensions
{
    public static IResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("A successful result cannot be converted to a problem response.");
        }

        var statusCode = result.Error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Results.Problem(
            title: result.Error.Code,
            detail: result.Error.Message,
            statusCode: statusCode);
    }

    public static IResult ToOkOrProblem<TValue>(this Result<TValue> result)
    {
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    public static IResult ToNoContentOrProblem(this Result result)
    {
        return result.IsSuccess ? Results.NoContent() : result.ToProblem();
    }
}
