namespace Nestor.Db.Models;

public class ObjectEntity
{
    public string Key { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;
}
