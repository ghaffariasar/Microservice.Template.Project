using Microsoft.EntityFrameworkCore;
using InventoryService.Domain.Entities;

namespace InventoryService.Infrastructure.Data;

/// <summary>
/// DbContext برای Inventory Service
/// این کلاس از InMemory Database به صورت پیش‌فرض استفاده می‌کند
/// می‌توان با تغییر تنظیمات به دیتابیس واقعی (SQL Server) تغییر داد
/// </summary>
public class InventoryDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // تنظیمات Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.StockQuantity).IsRequired();
            entity.Property(e => e.ReservedQuantity).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
            
            // Optimistic Concurrency Control با RowVersion
            // این از Race Condition در فشار زیاد جلوگیری می‌کند
            entity.Property(e => e.RowVersion)
                .IsRowVersion()  // SQL Server Timestamp
                .IsConcurrencyToken();  // EF Core Concurrency Token
        });
    }
}

