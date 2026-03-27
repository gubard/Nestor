using Gaia.Services;

namespace Nestor.Db.Services;

public interface IDbCache<in TPostRequest, in TGetResponse> : ICache<TPostRequest, TGetResponse>;

public abstract class EmptyDbCache<TPostRequest, TGetResponse>
    : EmptyCache<TPostRequest, TGetResponse>,
        IDbCache<TPostRequest, TGetResponse>;
