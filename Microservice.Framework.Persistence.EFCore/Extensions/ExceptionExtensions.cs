using Microsoft.EntityFrameworkCore;

namespace Microservice.Framework.Persistence.EFCore
{
    public static class ExceptionExtensions
    {
        public static bool IsUniqueConstraintViolation(this DbUpdateException e,
            IUniqueConstraintDetectionStrategy strategy)
        {
            return strategy.IsUniqueConstraintViolation(e);
        }
    }
}