namespace Nestor.Db.Models;

public interface IPostRequest
{
    EventEntity[] Events { get; set; }
}
