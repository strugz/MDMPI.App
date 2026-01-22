using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.CommonOldEntities.Entities;
using MDMPI.App.Core.Logistic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace MDMPI.App.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<RequestStandardModel> a_tblRequestStandardDelivery { get; set; }
        public DbSet<DocumentReferenceModel> a_tblRequestDocumentReference { get; set; }
        public DbSet<ImagePathModel> a_tblRequestImagePath { get; set; }
        public DbSet<RemarksModel> a_tblRequestRemarks { get; set; }
        public DbSet<ACCMSTModel> ACCMST_ { get; set; }
        public DbSet<MobileModel> a_tblMobile { get; set; }
        public DbSet<RequestPullOutReturnPickUpModel> a_tblRequestPullOutReturnPickUp { get; set; }
        public DbSet<RequestCounterModel> a_tblRequestCounters { get; set; }
        public DbSet<RequestPickUpModel> a_tblRequestPickUpMDMPI { get; set; }
        public DbSet<RequestAirSeaModel> a_tblRequestAirSea { get; set; }
        public DbSet<CategoryModel> a_tblCategory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RequestStandardModel>()
                .ToTable("a_tblRequestStandardDelivery", b => b.UseSqlOutputClause(false));

            modelBuilder.Entity<RequestPullOutReturnPickUpModel>()
                .ToTable("a_tblRequestPullOutReturnPickUp");

            modelBuilder.Entity<RequestPickUpModel>()
                .ToTable("a_tblRequestPickUpMDMPI");

            modelBuilder.Entity<RequestAirSeaModel>()
                .ToTable("a_tblRequestAirSea");
        }

        // call from a repository or controller
        public Task<List<ACCMSTModel>> GetAccMstByCodeAsync(string code) =>
            ACCMST_.FromSqlInterpolated($"EXEC dbo.usp_GetACCMST {code}")
                   .AsNoTracking()
                   .ToListAsync();

        public async Task DoSomethingAsync(string parameter)
        {
            var p1 = new SqlParameter("@p1", parameter ?? (object)DBNull.Value);
            await Database.ExecuteSqlRawAsync("EXEC dbo.usp_DoSomething @p1", p1);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is RequestAirSeaModel airSeaEntity)
                {
                    airSeaEntity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is RequestPickUpModel pickUpEntity)
                {
                    pickUpEntity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is RequestPullOutReturnPickUpModel pullOutEntity)
                {
                    pullOutEntity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
