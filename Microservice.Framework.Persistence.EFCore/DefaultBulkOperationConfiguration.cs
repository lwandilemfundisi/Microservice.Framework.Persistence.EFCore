namespace Microservice.Framework.Persistence.EFCore
{
    public class DefaultBulkOperationConfiguration : IBulkOperationConfiguration
    {
        public int DeletionBatchSize { get; }

        public DefaultBulkOperationConfiguration()
        {
            DeletionBatchSize = 1000;
        }
    }
}