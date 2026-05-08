using System;
using System.Threading.Tasks;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Common.Entities.Item;

namespace MDMPI.App.Data.Common.Services
{
    public class ItemIdGenerator : IItemIdGenerator
    {
        private readonly PostgreSqlAppDbContext _db;
        private readonly ILogger<ItemIdGenerator> _logger;

        public ItemIdGenerator(PostgreSqlAppDbContext db, ILogger<ItemIdGenerator> logger)
        {
            _db = db;
            _logger = logger;
            _logger.LogDebug("ItemIdGenerator in use: {Type}", GetType().FullName);
        }

        public async Task<long> GenerateAsync()
        {
            var yearMonth = DateTime.UtcNow.ToString("yyyyMM");

            var counter = await _db.a_tblItemCounters
                .FromSqlInterpolated($"""
                    select *
                    from public.a_tblitemcounters
                    where yearmonth = {yearMonth}
                    for update
                """)
                .FirstOrDefaultAsync();

            if (counter == null)
            {
                counter = new ItemCounterModel
                {
                    YearMonth = yearMonth,
                    LastNumber = 1
                };
                _db.a_tblItemCounters.Add(counter);
            }
            else
            {
                counter.LastNumber += 1;
                _db.a_tblItemCounters.Update(counter);
            }

            await _db.SaveChangesAsync();

            var id = long.Parse($"{yearMonth}{counter.LastNumber:0000}");
            _logger.LogDebug("Generated ItemID {ItemID} (YearMonth={YearMonth}, Seq={Seq})",
                id, yearMonth, counter.LastNumber);
            return id;
        }
    }
}