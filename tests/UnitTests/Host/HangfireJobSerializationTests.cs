using System.Linq.Expressions;
using GenclikMerkezi.Modules.Support.Infrastructure.Jobs;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using Hangfire.Common;
using Hangfire.Storage;

namespace GenclikMerkezi.UnitTests.Host;

// chore(deps) Newtonsoft.Json 13.0.4 pin (GHSA-5crp-9r3c-p9vr): GenclikMerkezi.Api registers every
// recurring job below with RecurringJob.AddOrUpdate<TJob>(..., job => job.ExecuteAsync(CancellationToken.None),
// ...) (Program.cs). That call - and later, a Hangfire.SqlServer worker reading the stored job back -
// goes through exactly the InvocationData.SerializeJob -> SerializePayload -> DeserializePayload ->
// DeserializeJob path exercised here, which is where Hangfire.Common.SerializationHelper's
// Newtonsoft.Json-based serialization actually runs. This proves that pinning Newtonsoft.Json straight
// to 13.0.4 - a major-version jump from the 11.0.1 Hangfire.Core 1.8.15 itself depends on - does not
// change what gets persisted or how a stored job is read back: method and argument data survive the
// round trip unchanged for every recurring job type in the solution (grep for RecurringJob.AddOrUpdate
// in Program.cs to find the full set).
public sealed class HangfireJobSerializationTests
{
    [Fact]
    public void CloseOverdueSupportTicketsJob_round_trips_through_Hangfire_serialization() =>
        AssertRoundTrips<CloseOverdueSupportTicketsJob>(job => job.ExecuteAsync(CancellationToken.None));

    [Fact]
    public void CleanupStaleNotFoundLogsJob_round_trips_through_Hangfire_serialization() =>
        AssertRoundTrips<CleanupStaleNotFoundLogsJob>(job => job.ExecuteAsync(CancellationToken.None));

    [Fact]
    public void CleanupUnusedContentTagsJob_round_trips_through_Hangfire_serialization() =>
        AssertRoundTrips<CleanupUnusedContentTagsJob>(job => job.ExecuteAsync(CancellationToken.None));

    [Fact]
    public void PermanentlyDeleteExpiredTrashJob_round_trips_through_Hangfire_serialization() =>
        AssertRoundTrips<PermanentlyDeleteExpiredTrashJob>(job => job.ExecuteAsync(CancellationToken.None));

    [Fact]
    public void ArchiveClosedFormSubmissionsJob_round_trips_through_Hangfire_serialization() =>
        AssertRoundTrips<ArchiveClosedFormSubmissionsJob>(job => job.ExecuteAsync(CancellationToken.None));

    [Fact]
    public void AnonymizeExpiredFormSubmissionsJob_round_trips_through_Hangfire_serialization() =>
        AssertRoundTrips<AnonymizeExpiredFormSubmissionsJob>(job => job.ExecuteAsync(CancellationToken.None));

    // Mirrors what Hangfire.SqlServer actually persists/reads for a stored job: SerializeJob captures
    // Type/Method/ParameterTypes and serializes each argument; SerializePayload(excludeArguments: false)
    // is the JSON string written to storage; DeserializePayload + DeserializeJob is the read-back path
    // a worker uses before invoking the method.
    private static void AssertRoundTrips<TJob>(Expression<Action<TJob>> methodCall)
    {
        var job = Job.FromExpression(methodCall);

        var payload = InvocationData.SerializeJob(job).SerializePayload(excludeArguments: false);
        var roundTrippedJob = InvocationData.DeserializePayload(payload).DeserializeJob();

        Assert.Equal(job.Type, roundTrippedJob.Type);
        Assert.Equal(job.Method.Name, roundTrippedJob.Method.Name);
        Assert.Equal(job.Method.DeclaringType, roundTrippedJob.Method.DeclaringType);
        Assert.Equal(
            job.Method.GetParameters().Select(p => p.ParameterType),
            roundTrippedJob.Method.GetParameters().Select(p => p.ParameterType));
        Assert.Equal(job.Args.Count, roundTrippedJob.Args.Count);
        Assert.Equal(job.Args, roundTrippedJob.Args);
    }
}
