using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Framework.Persistence.EFCore.Test
{
    public class TestDbContextProvider : IDbContextProvider<TestDbContext>, IDisposable
    {
        private readonly DbContextOptions<TestDbContext> _options;

        public TestDbContextProvider()
        {
            _options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlServer("Data Source=AGPLPF14YDG7\\SQLEXPRESS;Initial Catalog=AdventureWorksLT;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")
                .Options;
        }

        public TestDbContext CreateContext()
        {
            return new TestDbContext(_options);
        }

        public void Dispose()
        {
        }
    }
}
