using System.IO;
using Gaia.Services;
using Microsoft.EntityFrameworkCore;

namespace Nestor.Db.Services;

public interface IDbContextFactory : IFactory<FileInfo, DbContext>;
