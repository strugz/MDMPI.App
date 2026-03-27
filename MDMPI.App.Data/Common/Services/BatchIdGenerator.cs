using System;
using System.Threading.Tasks;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MDMPI.App.Core.Common.Interfaces;

namespace MDMPI.App.Data.Common.Services
{
    public class BatchIdGenerator : IBatchIdGenerator
    {
        private readonly AppDbContext _db;
        private readonly ILogger<BatchIdGenerator> _logger;

        public BatchIdGenerator(AppDbContext db, ILogger<BatchIdGenerator> logger)
        {
            _db = db;
            _logger = logger;
            _logger.LogDebug("BatchIdGenerator in use: {Type}", GetType().FullName);
        }

        public async Task<long> GenerateAsync()
        {
            var yearMonth = DateTime.UtcNow.ToString("yyyyMM");

            var counter = await _db.a_tblBatchCounters
                .FromSqlInterpolated($"""
                    SELECT * FROM a_tblBatchCounters WITH (UPDLOCK, HOLDLOCK)
                    WHERE YearMonth = {yearMonth}
                """)
                .FirstOrDefaultAsync();

            if (counter == null)
            {
                counter = new BatchCounterModel
                {
                    YearMonth = yearMonth,
                    LastNumber = 1
                };
                _db.a_tblBatchCounters.Add(counter);
            }
            else
            {
                counter.LastNumber += 1;
                _db.a_tblBatchCounters.Update(counter);
            }

            await _db.SaveChangesAsync();

            var id = long.Parse($"{yearMonth}{counter.LastNumber:0000}");
            _logger.LogDebug("Generated BatchID {BatchID} (YearMonth={YearMonth}, Seq={Seq})",
                id, yearMonth, counter.LastNumber);
            return id;
        }
    }
}