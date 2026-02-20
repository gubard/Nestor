using System.Data.Common;
using Gaia.Services;

namespace Nestor.Db.Services;

public interface IDbConnectionFactory : IFactory<DbConnection>;
