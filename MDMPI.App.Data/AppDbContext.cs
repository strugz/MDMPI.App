using MDMPI.App.Core.Collection.Entities;
using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Logistic.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MDMPI.App.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ACCMSTModel> ACCMST_ { get; set; }
        public DbSet<CollectionTransactionDetailsModel> a_tblCollectionTransactionDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ACCMSTModel>()
                .ToTable("ACCMST_");

            modelBuilder.Entity<CollectionTransactionDetailsModel>()
                .ToTable("a_tblCollectionTransactionDetails", b => b.UseSqlOutputClause(false));
        }

        public Task<List<ACCMSTModel>> GetAccMstByCodeAsync(string code) =>
            ACCMST_.FromSqlInterpolated($"EXEC dbo.usp_GetACCMST {code}")
                   .AsNoTracking()
                   .ToListAsync();

        public async Task DoSomethingAsync(string parameter)
        {
            var p1 = new SqlParameter("@p1", parameter ?? (object)DBNull.Value);
            await Database.ExecuteSqlRawAsync("EXEC dbo.usp_DoSomething @p1", p1);
        }
    }
}
