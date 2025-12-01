using System.Linq;
using Nestor.Db.Models;

namespace Nestor.Db.Helpers;

public static class EventEntityExtension
{
    public static IQueryable<EventEntity> GetProperty(this IQueryable<EventEntity> query, string entityType, string entityProperty)
    {
        return query.Where(y => query.GroupBy(x => x.EntityId)
           .Select(e =>
                e.Where(x =>
                        x.EntityId == e.Key
                     && x.EntityProperty == entityProperty
                     && x.EntityType == entityType)
                   .Max(x => x.Id))
           .Contains(y.Id));
    }
}