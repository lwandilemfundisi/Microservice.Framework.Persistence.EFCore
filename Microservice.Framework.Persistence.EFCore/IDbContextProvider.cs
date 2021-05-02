using Microsoft.EntityFrameworkCore;

namespace Microservice.Framework.Persistence.EFCore
{
    public interface IDbContextProvider<out TDbContext> where TDbContext : DbContext
    {
        TDbContext CreateContext();
    }
}