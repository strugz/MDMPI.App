using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Entities.Item;
using MDMPI.App.Core.Logistic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MDMPI.App.Data
{
    public class PostgreSqlAppDbContext : DbContext
    {
        public PostgreSqlAppDbContext(DbContextOptions<PostgreSqlAppDbContext> options) : base(options)
        {
        }

        public DbSet<BackloadCounterModel> a_tblBackloadCounters { get; set; }
        public DbSet<BatchCounterModel> a_tblBatchCounters { get; set; }
        public DbSet<CategoryModel> a_tblCategory { get; set; }
        public DbSet<ItemCounterModel> a_tblItemCounters { get; set; }
        public DbSet<MobileModel> a_tblMobile { get; set; }
        public DbSet<RequestAirSeaModel> a_tblRequestAirSea { get; set; }
        public DbSet<RequestAirSeaHistoryModel> a_tblRequestAirSea_History { get; set; }
        public DbSet<RequestBackloadModel> a_tblRequestBackload { get; set; }
        public DbSet<RequestCounterModel> a_tblRequestCounters { get; set; }
        public DbSet<DocumentReferenceModel> a_tblRequestDocumentReference { get; set; }
        public DbSet<ImagePathModel> a_tblRequestImagePath { get; set; }
        public DbSet<RequestPickUpModel> a_tblRequestPickUpMDMPI { get; set; }
        public DbSet<RequestPickUpHistoryModel> a_tblRequestPickUpMDMPI_History { get; set; }
        public DbSet<RequestPullOutReturnPickUpModel> a_tblRequestPullOutReturnPickUp { get; set; }
        public DbSet<RequestPullOutReturnPickUpHistoryModel> a_tblRequestPullOutReturnPickUp_History { get; set; }
        public DbSet<RemarksModel> a_tblRequestRemarks { get; set; }
        public DbSet<RequestStandardModel> a_tblRequestStandardDelivery { get; set; }
        public DbSet<RequestStandardHistoryModel> a_tblRequestStandardDeliveryHistory { get; set; }
        public DbSet<ItemModel> a_tblRequestStandardItem { get; set; }
        public DbSet<ItemBatchModel> a_tblRequestStandardItemBatch { get; set; }
        public DbSet<LoseItemModel> a_tblLoseItem { get; set; }
        public DbSet<PickUpItemCategoryModel> a_tblRequestPickUpItemCategory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("public");

            modelBuilder.Entity<BackloadCounterModel>().ToTable("a_tblbackloadcounters", "public");
            modelBuilder.Entity<BatchCounterModel>().ToTable("a_tblbatchcounters", "public");
            modelBuilder.Entity<CategoryModel>().ToTable("a_tblcategory", "public");
            modelBuilder.Entity<ItemCounterModel>().ToTable("a_tblitemcounters", "public");
            modelBuilder.Entity<MobileModel>().ToTable("a_tblmobile", "public");
            modelBuilder.Entity<RequestBackloadModel>().ToTable("a_tblrequestbackload", "public");
            modelBuilder.Entity<RequestCounterModel>().ToTable("a_tblrequestcounters", "public");
            modelBuilder.Entity<DocumentReferenceModel>().ToTable("a_tblrequestdocumentreference", "public");
            modelBuilder.Entity<ImagePathModel>().ToTable("a_tblrequestimagepath", "public");
            modelBuilder.Entity<RemarksModel>().ToTable("a_tblrequestremarks", "public");
            modelBuilder.Entity<RequestStandardHistoryModel>().ToTable("a_tblrequeststandarddeliveryhistory", "public");
            modelBuilder.Entity<RequestAirSeaHistoryModel>().ToTable("a_tblrequestairsea_history", "public");
            modelBuilder.Entity<RequestPickUpHistoryModel>().ToTable("a_tblrequestpickupmdmpi_history", "public");
            modelBuilder.Entity<RequestPullOutReturnPickUpHistoryModel>().ToTable("a_tblrequestpulloutreturnpickup_history", "public");
            modelBuilder.Entity<ItemModel>().ToTable("a_tblrequeststandarditem", "public");
            modelBuilder.Entity<ItemBatchModel>().ToTable("a_tblrequeststandarditembatch", "public");
            modelBuilder.Entity<LoseItemModel>().ToTable("a_tblloseitem", "public");
            modelBuilder.Entity<PickUpItemCategoryModel>().ToTable("a_tblrequestpickupitemcategory", "public");

            modelBuilder.Entity<RequestStandardModel>(entity =>
            {
                entity.ToTable("a_tblrequeststandarddelivery", "public");
                entity.Ignore(x => x.Client);
                entity.Ignore(x => x.Image);
                entity.Ignore(x => x.Signature);
            });

            modelBuilder.Entity<RequestAirSeaModel>(entity =>
            {
                entity.ToTable("a_tblrequestairsea", "public");
                entity.Ignore(x => x.Client);
                entity.Ignore(x => x.Signature);
            });

            modelBuilder.Entity<RequestPickUpModel>(entity =>
            {
                entity.ToTable("a_tblrequestpickupmdmpi", "public");
                entity.Ignore(x => x.Client);
            });

            modelBuilder.Entity<RequestPullOutReturnPickUpModel>(entity =>
            {
                entity.ToTable("a_tblrequestpulloutreturnpickup", "public");
                entity.Ignore(x => x.Client);
                entity.Ignore(x => x.Signature);
                entity.Property(x => x.IRRFDate).HasColumnType("date");
            });

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    property.SetColumnName(property.Name.ToLowerInvariant());
                }
            }
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

            NormalizeTrackedDateTimesToUtc();

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void NormalizeTrackedDateTimesToUtc()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                NormalizeEntryDateTimesToUtc(entry);
            }
        }

        private static void NormalizeEntryDateTimesToUtc(EntityEntry entry)
        {
            foreach (var property in entry.Properties)
            {
                var clrType = property.Metadata.ClrType;
                var underlyingType = Nullable.GetUnderlyingType(clrType);

                if (clrType != typeof(DateTime) && underlyingType != typeof(DateTime))
                {
                    continue;
                }

                if (property.CurrentValue is DateTime dateTime)
                {
                    property.CurrentValue = NormalizeToUtc(dateTime);
                }
            }
        }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }
    }
}
