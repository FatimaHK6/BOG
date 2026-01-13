using BOG.DbModel.Entities;
using Microsoft.EntityFrameworkCore;

namespace BOG.DbModel;

/// <summary>
/// Application's main DbContext for Entity Framework Core.
/// Follows Single Responsibility Principle - manages only database configuration and entity mapping.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// DbSet for User entities.
    /// </summary>
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations using fluent API or separate configuration classes
        // This can be extended with IEntityTypeConfiguration implementations

        ConfigureUserEntity(modelBuilder);
    }

    /// <summary>
    /// Configures the User entity mapping and constraints.
    /// Following Single Responsibility Principle - configuration isolated in separate method.
    /// </summary>
    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CreatedDate)
                .IsRequired();

            entity.Property(e => e.ModifiedDate)
                .IsRequired();

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Add unique constraint on email
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        });
    }

    /// <summary>
    /// Override SaveChangesAsync to update ModifiedDate on all modified entities.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates ModifiedDate for all modified entities.
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && e.State != EntityState.Unchanged);

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            entity.ModifiedDate = DateTime.UtcNow;
        }
    }
}
