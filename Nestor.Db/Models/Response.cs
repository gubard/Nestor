namespace Nestor.Db.Models;

public interface IResponse
{
    EventEntity[] Events { get; set; }
}