using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data;

/// <summary>
/// DbContext برای Order Service
/// این کلاس از InMemory Database به صورت پیش‌فرض استفاده می‌کند
/// می‌توان با تغییر تنظیمات به دیتابیس واقعی (SQL Server) تغییر داد
/// </summary>
public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // تنظیمات Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.OrderDate).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
            entity.HasMany(e => e.Items)
                  .WithOne()
                  .HasForeignKey("OrderId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // تنظیمات OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
        });
    }
}

