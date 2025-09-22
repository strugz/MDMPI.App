using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.CommonOldEntities.Entities;
using MDMPI.App.Core.Logistic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RequestStandardModel>()
                .ToTable("a_tblRequestStandardDelivery", b => b.UseSqlOutputClause(false));

            modelBuilder.Entity<RequestPullOutReturnPickUpModel>()
                .ToTable("a_tblRequestPullOutReturnPickUp");
        }
    }
}
