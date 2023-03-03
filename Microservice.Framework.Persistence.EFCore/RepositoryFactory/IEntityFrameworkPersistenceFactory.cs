using Microsoft.EntityFrameworkCore;

namespace Microservice.Framework.Persistence.EFCore.RepositoryFactory
{
    public interface IEntityFrameworkPersistenceFactory<TDbContext> : IPersistenceFactory
        where TDbContext : DbContext
    {
    }
}
