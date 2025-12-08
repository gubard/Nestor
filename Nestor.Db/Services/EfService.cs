using System;
using System.Threading;
using System.Threading.Tasks;
using Gaia.Services;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public interface
    IEfService<in TGetRequest, in TPostRequest, TGetResponse, TPostResponse> :
    IService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    where TGetResponse : IValidationErrors, new()
    where TPostResponse : IValidationErrors, new()
{
    ValueTask SaveEventsAsync(ReadOnlyMemory<EventEntity> events, CancellationToken ct);
    ValueTask<long> GetLastIdAsync(CancellationToken ct);
}