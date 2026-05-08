using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Data.Common.Repositories
{
    public class MobileRepository : IMobileRepository
    {
        private readonly PostgreSqlAppDbContext _db;
        public MobileRepository(PostgreSqlAppDbContext db) => _db = db;

        public async Task<List<MobileDto>> GetAllMobilesAsync()
        {
            var mobiles = await _db.a_tblMobile
                .Select(m => new MobileDto
                {
                    MobileID = m.MobileID,
                    MobileName = m.MobileName
                })
                .ToListAsync();
            return mobiles;
        }
    }
}
