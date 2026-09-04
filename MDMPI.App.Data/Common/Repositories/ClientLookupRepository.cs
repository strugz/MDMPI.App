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

            // UNION of ACCMST_ (main: clinics/hospitals) and DLRMST (dealers),
            // projected to the same shape. Source 0 = ACCMST_, 1 = DLRMST, so
            // ACCMST_ wins when the same id exists in both tables.
            var rows = await _db.ACCMST_
                .AsNoTracking()
                .Where(c => c.ACCMID != null && normalizedIds.Contains(c.ACCMID))
                .Select(c => new
                {
                    Source = 0,
                    Id = c.ACCMID,
                    Code = c.ACCMSC,
                    Name = c.ACCMNM,
                    BusinessCode = c.ACCMBC,
                    Address = c.ACCMAD,
                    Phone = c.ACCMPH,
                    Email = c.ACCMEM,
                    Website = c.ACCMWS,
                    Status = c.ACCSTS,
                    Owner = c.ACCOWN
                })
                .Union(_db.DLRMST
                    .AsNoTracking()
                    .Where(d => d.DLRMID != null && normalizedIds.Contains(d.DLRMID))
                    .Select(d => new
                    {
                        Source = 1,
                        Id = d.DLRMID,
                        Code = d.DLRMSC,
                        Name = d.DLRMNM,
                        BusinessCode = d.DLRMBC,
                        Address = d.DLRMAD,
                        Phone = d.DLRMPH,
                        Email = d.DLRMEM,
                        Website = d.DLRMWS,
                        Status = d.DLRSTS,
                        Owner = d.DLROWN
                    }))
                .ToListAsync();

            var result = new Dictionary<string, ACCMSTDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows.OrderBy(r => r.Source))
            {
                if (row.Id == null || result.ContainsKey(row.Id))
                {
                    continue;
                }

                result[row.Id] = new ACCMSTDto
                {
                    ACCMID = row.Id,
                    ACCMSC = row.Code,
                    ACCMNM = row.Name != null ? row.Name.ToProperCase() : null,
                    ACCMBC = row.BusinessCode,
                    ACCMAD = row.Address,
                    ACCMPH = row.Phone,
                    ACCMEM = row.Email,
                    ACCMWS = row.Website,
                    ACCSTS = row.Status,
                    ACCOWN = row.Owner
                };
            }

            return result;
        }
    }
}
