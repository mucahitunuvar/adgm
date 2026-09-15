using FluentValidation;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var failures = new List<FluentValidation.Results.ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        var errorMessage = string.Join(" ", failures.Select(f => f.ErrorMessage));
        var error = Error.Validation("Validation.Failed", errorMessage);

        var failureResultType = typeof(TResponse);

        if (failureResultType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var valueType = failureResultType.GetGenericArguments()[0];
        var failureMethod = typeof(Result)
            .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
            .MakeGenericMethod(valueType);

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }
}
