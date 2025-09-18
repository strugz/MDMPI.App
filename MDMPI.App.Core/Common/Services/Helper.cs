using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Common.Services
{
    public  class Helper
    {
        public static void UpdateIfNotNull<T>(Action<T> setter, T? value)
        {
            if (value != null)
                setter(value);
        }

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
