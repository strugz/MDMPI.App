using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.CommonOldEntities.Entities;
using MDMPI.App.Core.Logistic.Entities;
using Microsoft.EntityFrameworkCore;

namespace MDMPI.App.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<RequestStandardModel> a_tblRequest { get; set; }
        public DbSet<DocumentReferenceModel> a_tblRequestDocumentReference { get; set; }
        public DbSet<ImageModel> a_tblRequestImage { get; set; }
        public DbSet<SignatureModel> a_tblRequestReceiverSignature { get; set; }
        public DbSet<RemarksModel> a_tblRequestRemarks { get; set; }
        public DbSet<ACCMSTModel> ACCMST_ { get; set; }
        public DbSet<MobileModel> a_tblMobile { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<RequestStandardModel>()
                .ToTable("a_tblRequest", b => b.UseSqlOutputClause(false));
        }
    }
}
