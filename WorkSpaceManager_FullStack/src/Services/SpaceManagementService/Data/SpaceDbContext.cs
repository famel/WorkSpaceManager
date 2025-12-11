using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.SpaceManagementService.Data;

public class SpaceDbContext : DbContext
{
    public SpaceDbContext(DbContextOptions<SpaceDbContext> options) : base(options)
    {
    }

    public DbSet<Building> Buildings { get; set; }
    public DbSet<Floor> Floors { get; set; }
    public DbSet<Desk> Desks { get; set; }
    public DbSet<MeetingRoom> MeetingRooms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Building entity
        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Floor entity
        modelBuilder.Entity<Floor>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.BuildingId);
            entity.HasIndex(e => e.FloorNumber);
            entity.HasIndex(e => e.IsActive);

            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Building)
                .WithMany(b => b.Floors)
                .HasForeignKey(e => e.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Desk entity
        modelBuilder.Entity<Desk>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.FloorId);
            entity.HasIndex(e => e.DeskNumber);
            entity.HasIndex(e => e.IsAvailable);

            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Floor)
                .WithMany(f => f.Desks)
                .HasForeignKey(e => e.FloorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure MeetingRoom entity
        modelBuilder.Entity<MeetingRoom>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.FloorId);
            entity.HasIndex(e => e.RoomNumber);
            entity.HasIndex(e => e.IsAvailable);
            entity.HasIndex(e => e.Capacity);

            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Floor)
                .WithMany(f => f.MeetingRooms)
                .HasForeignKey(e => e.FloorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
