using Gaia.Services;

namespace Nestor.Db.Models;

public interface IResponse : IValidationErrors;

public interface IPostResponse : IResponse
{
    bool IsEventSaved { get; set; }
}
