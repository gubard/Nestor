namespace Nestor.Db.Models;

public interface IResponse;

public interface IPostResponse : IResponse
{
    bool IsEventSaved { get; set; }
}
