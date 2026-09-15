using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Common.Interfaces;
using OrderFlow.Application.Common.Models;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Infrastracture.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderDashboardReadModel> OrderDashboardReadModels => Set<OrderDashboardReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Order Configuration (Write Model)
        modelBuilder.Entity<Order>(builder =>
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.CustomerName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.Status)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v));

            builder.Property(o => o.CreatedAtUtc)
                .IsRequired();

            // Configure backing field for encapsulated _items collection
            builder.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(o => o.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // OrderItem Configuration (Write Model)
        modelBuilder.Entity<OrderItem>(builder =>
        {
            builder.ToTable("OrderItems");
            builder.HasKey(i => i.Id);

            builder.Property(i => i.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.Property(i => i.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Ignore(i => i.LineTotal);
        });

        // OrderDashboardReadModel Configuration (SQL Server Materialized View / Read Model Table)
        modelBuilder.Entity<OrderDashboardReadModel>(builder =>
        {
            builder.ToTable("OrderDashboardReadModel");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.CustomerName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(m => m.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.LastUpdatedUtc)
                .IsRequired();
        });
    }
}
