using MDMPI.App.Common.Utilities;
using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MDMPI.App.Data.Common.Repositories
{
    public class ClientLookupRepository : IClientLookupRepository
    {
        private readonly AppDbContext _db;

        public ClientLookupRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Dictionary<string, ACCMSTDto>> GetByIdsAsync(IEnumerable<string> clientIds)
        {
            ArgumentNullException.ThrowIfNull(clientIds);

            var normalizedIds = clientIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalizedIds.Count == 0)
            {
                return new Dictionary<string, ACCMSTDto>(StringComparer.OrdinalIgnoreCase);
            }

            return await _db.ACCMST_
                .AsNoTracking()
                .Where(c => c.ACCMID != null && normalizedIds.Contains(c.ACCMID))
                .Select(c => new ACCMSTDto
                {
                    ACCMID = c.ACCMID,
                    ACCMSC = c.ACCMSC,
                    ACCMNM = c.ACCMNM != null ? c.ACCMNM.ToProperCase() : null,
                    ACCMBC = c.ACCMBC,
                    ACCMAD = c.ACCMAD,
                    ACCMPH = c.ACCMPH,
                    ACCMEM = c.ACCMEM,
                    ACCMWS = c.ACCMWS,
                    ACCSTS = c.ACCSTS,
                    ACCOWN = c.ACCOWN
                })
                .ToDictionaryAsync(c => c.ACCMID!, StringComparer.OrdinalIgnoreCase);
        }
    }
}
