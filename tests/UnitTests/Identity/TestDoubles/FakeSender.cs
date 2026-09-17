using MediatR;

namespace GenclikMerkezi.UnitTests.Identity.TestDoubles;

// Minimal ISender fake for IdentityServiceTests.CreateUserAsync: records the last request sent and
// returns a pre-configured response, without wiring a real MediatR pipeline.
public sealed class FakeSender : ISender
{
    private readonly Dictionary<Type, object?> _responsesByRequestType = [];

    public object? LastRequest { get; private set; }

    public void SetResponse<TRequest>(object? response) => _responsesByRequestType[typeof(TRequest)] = response;

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        LastRequest = request;

        if (!_responsesByRequestType.TryGetValue(request.GetType(), out var response))
        {
            throw new InvalidOperationException($"No fake response configured for {request.GetType().Name}.");
        }

        return Task.FromResult((TResponse)response!);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest =>
        throw new NotImplementedException();

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
