using Microsoft.EntityFrameworkCore;
using WorkSpaceManager.Shared.Models;

namespace WorkSpaceManager.BookingService.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Desk> Desks { get; set; }
    public DbSet<MeetingRoom> MeetingRooms { get; set; }
    public DbSet<Floor> Floors { get; set; }
    public DbSet<Building> Buildings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Booking entity
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.DeskId);
            entity.HasIndex(e => e.MeetingRoomId);
            entity.HasIndex(e => e.BookingDate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.BookingDate, e.Status });

            // Query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Relationships
            entity.HasOne(e => e.Desk)
                .WithMany(d => d.Bookings)
                .HasForeignKey(e => e.DeskId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.MeetingRoom)
                .WithMany(m => m.Bookings)
                .HasForeignKey(e => e.MeetingRoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.KeycloakUserId).IsUnique();

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Desk entity
        modelBuilder.Entity<Desk>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.FloorId);
            entity.HasIndex(e => e.DeskNumber);
            entity.HasIndex(e => e.IsAvailable);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure MeetingRoom entity
        modelBuilder.Entity<MeetingRoom>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.FloorId);
            entity.HasIndex(e => e.RoomNumber);
            entity.HasIndex(e => e.IsAvailable);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Floor entity
        modelBuilder.Entity<Floor>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.BuildingId);
            entity.HasIndex(e => e.FloorNumber);

            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Building)
                .WithMany(b => b.Floors)
                .HasForeignKey(e => e.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Building entity
        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.Name);

            entity.HasQueryFilter(e => !e.IsDeleted);
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
