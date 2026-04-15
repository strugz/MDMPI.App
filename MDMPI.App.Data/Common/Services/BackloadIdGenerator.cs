using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Data.Common.Services
{
    public class BackloadIdGenerator : IBackloadIdGenerator
    {
        private readonly AppDbContext _db;
        private readonly ILogger<BackloadIdGenerator> _logger;

        public BackloadIdGenerator(AppDbContext db, ILogger<BackloadIdGenerator> logger)
        {
            _db = db;
            _logger = logger;
            _logger.LogDebug("BackloadIdGenerator in use: {Type}", GetType().FullName);
        }

        public async Task<long> GenerateAsync()
        {
            var yearMonth = DateTime.UtcNow.ToString("yyyyMM");

            var counter = await _db.a_tblBackloadCounters
                .FromSqlInterpolated($"""
                    SELECT * FROM a_tblBackloadCounters WITH (UPDLOCK, HOLDLOCK)
                    WHERE YearMonth = {yearMonth}
                """)
                .FirstOrDefaultAsync();

            if (counter == null)
            {
                counter = new BackloadCounterModel
                {
                    YearMonth = yearMonth,
                    LastNumber = 1
                };
                _db.a_tblBackloadCounters.Add(counter);
            }
            else
            {
                counter.LastNumber += 1;
                _db.a_tblBackloadCounters.Update(counter);
            }

            await _db.SaveChangesAsync();

            var id = long.Parse($"{yearMonth}{counter.LastNumber:0000}");
            _logger.LogDebug("Generated BackLoadID {BackLoadID} (YearMonth={YearMonth}, Seq={Seq})", id, yearMonth, counter.LastNumber);
            return id;
        }
    }
}
