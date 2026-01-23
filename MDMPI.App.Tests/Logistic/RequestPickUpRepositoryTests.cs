using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Logistic.DTOs.RequestPickUp;
using MDMPI.App.Data;
using MDMPI.App.Data.Logistic.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace MDMPI.App.Tests.Logistic
{
    public class RequestPickUpRepositoryTests
    {
        private static (AppDbContext db, SqliteConnection conn) CreateDbContext()
        {
            var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conn)
                .Options;
            var db = new AppDbContext(options);
            db.Database.EnsureCreated();
            return (db, conn);
        }

        private class FixedIdGenerator : IRequestIdGenerator
        {
            private long _next;
            public FixedIdGenerator(long start) { _next = start; }
            public Task<long> GenerateAsync() => Task.FromResult(_next++);
        }

        [Fact]
        public async Task InsertAsync_InsertsEntity_WithDefaults()
        {
            var (db, conn) = CreateDbContext();
            try
            {
                var repo = new RequestPickUpRepository(db, NullLogger<RequestPickUpRepository>.Instance, new FixedIdGenerator(1000));

                var dto = new InsertRequestPickUpDto
                {
                    // minimal insert, no PreparedBy/ReleasedBy on insert
                    Status = null
                };

                var inserted = await repo.InsertAsync(dto);

                Assert.NotNull(inserted);
                var saved = await db.a_tblRequestPickUpMDMPI.AsNoTracking().FirstOrDefaultAsync();
                Assert.NotNull(saved);
                Assert.Equal(1000, saved!.RequestID);
                Assert.Equal("New Request", saved.Status);
                Assert.Null(saved.PreparedBy);
                Assert.Null(saved.ReleasedBy);
            }
            finally
            {
                conn.Close();
            }
        }

        [Fact]
        public async Task UpdateAsync_UpdatesOnlyProvidedFields()
        {
            var (db, conn) = CreateDbContext();
            try
            {
                var repo = new RequestPickUpRepository(db, NullLogger<RequestPickUpRepository>.Instance, new FixedIdGenerator(2000));

                var insertDto = new InsertRequestPickUpDto { Status = "New Request" };
                await repo.InsertAsync(insertDto);
                var existing = await db.a_tblRequestPickUpMDMPI.AsNoTracking().FirstAsync();

                // Step 1: move to Item Prepared, can set PreparedBy and ItemPreparedEndAt
                var updateToPrepared = new UpdateRequestPickUpDto
                {
                    RequestID = existing.RequestID,
                    PreparedBy = "Jane",
                    Status = "Item Prepared",
                    ItemPreparedEndAt = DateTime.UtcNow
                };

                var ok1 = await repo.UpdateAsync(updateToPrepared);
                Assert.True(ok1);
                var afterPrepared = await db.a_tblRequestPickUpMDMPI.AsNoTracking().FirstAsync();
                Assert.Equal("Jane", afterPrepared.PreparedBy);
                Assert.Equal("Item Prepared", afterPrepared.Status);
                Assert.NotNull(afterPrepared.ItemPreparedEndAt);

                // Step 2: move to Delivered, can set Remarks and ReceivedBy
                var updateToDelivered = new UpdateRequestPickUpDto
                {
                    RequestID = existing.RequestID,
                    Status = "Delivered",
                    Remarks = "Done",
                    ReceivedBy = "Warehouse"
                };

                var ok2 = await repo.UpdateAsync(updateToDelivered);
                Assert.True(ok2);

                var saved = await db.a_tblRequestPickUpMDMPI.AsNoTracking().FirstAsync();
                Assert.Equal("Jane", saved.PreparedBy); // unchanged
                Assert.Equal("Delivered", saved.Status);
                Assert.Equal("Done", saved.Remarks);
                Assert.Equal("Warehouse", saved.ReceivedBy);
                Assert.NotNull(saved.UpdatedAt);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
