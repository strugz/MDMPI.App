using System;
using System.Threading.Tasks;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MDMPI.App.Core.Logistic.Interfaces;

namespace MDMPI.App.Data.Common.Services
{
    public class RequestIdGenerator : IRequestIdGenerator
    {
        private readonly AppDbContext _db;
        private readonly ILogger<RequestIdGenerator> _logger;

        public RequestIdGenerator(AppDbContext db, ILogger<RequestIdGenerator> logger)
        {
            _db = db;
            _logger = logger;
            _logger.LogDebug("RequestIdGenerator in use: {Type}", GetType().FullName);
        }

        public async Task<long> GenerateAsync()
        {
            // Expect caller (repository) to have started a transaction if atomicity required.
            var yearMonth = DateTime.UtcNow.ToString("yyyyMM");

            // Lock current month row (or range) in caller transaction using UPDLOCK + HOLDLOCK.
            var counter = await _db.a_tblRequestCounters
                .FromSqlInterpolated($"""
                    SELECT * FROM a_tblRequestCounters WITH (UPDLOCK, HOLDLOCK)
                    WHERE YearMonth = {yearMonth}
                """)
                .FirstOrDefaultAsync();

            if (counter == null)
            {
                counter = new RequestCounterModel
                {
                    YearMonth = yearMonth,
                    LastNumber = 1
                };
                _db.a_tblRequestCounters.Add(counter);
            }
            else
            {
                counter.LastNumber += 1;
                _db.a_tblRequestCounters.Update(counter);
            }

            await _db.SaveChangesAsync();

            var id = long.Parse($"{yearMonth}{counter.LastNumber:0000}");
            _logger.LogDebug("Generated RequestID {RequestID} (YearMonth={YearMonth}, Seq={Seq})",
                id, yearMonth, counter.LastNumber);
            return id;
        }
    }
}