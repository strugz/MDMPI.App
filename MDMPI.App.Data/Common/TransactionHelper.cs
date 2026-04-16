using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Data.Common
{
    public static class TransactionHelper
    {
        public static async Task RollbackTransactionAsync(IDbContextTransaction transaction, ILogger logger, Exception ex, string contextMessage)
        {
            try
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Transaction rolled back: {ContextMessage}", contextMessage);
            }
            catch (Exception rollbackEx)
            {
                logger.LogError(rollbackEx, "Error during transaction rollback: {ContextMessage}", contextMessage);
            }
        }
    }
}
