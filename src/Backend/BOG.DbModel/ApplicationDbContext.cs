using BOG.DbModel.Entities;
using BOG.DbModel.Entities.CaseRegistration;
using BOG.DbModel.Entities.Common;
using BOG.DbModel.Entities.Identity;
using BOG.DbModel.Entities.Lookups;
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

    #region Identity DbSets

    /// <summary>
    /// DbSet for Role entities.
    /// </summary>
    public DbSet<Role> Roles { get; set; }

    /// <summary>
    /// DbSet for UserRole entities.
    /// </summary>
    public DbSet<UserRole> UserRoles { get; set; }

    /// <summary>
    /// DbSet for Court entities.
    /// </summary>
    public DbSet<Court> Courts { get; set; }

    /// <summary>
    /// DbSet for Department entities.
    /// </summary>
    public DbSet<Department> Departments { get; set; }

    /// <summary>
    /// DbSet for UserDepartment entities.
    /// </summary>
    public DbSet<UserDepartment> UserDepartments { get; set; }

    #endregion

    #region Lookup DbSets

    public DbSet<PlaintiffType> PlaintiffTypes { get; set; }
    public DbSet<DefendantType> DefendantTypes { get; set; }
    public DbSet<RepresentativeType> RepresentativeTypes { get; set; }
    public DbSet<RequestStatus> RequestStatuses { get; set; }
    public DbSet<IdentityType> IdentityTypes { get; set; }
    public DbSet<AttachmentType> AttachmentTypes { get; set; }
    public DbSet<DataSource> DataSources { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<GovernmentAgency> GovernmentAgencies { get; set; }
    public DbSet<LicenseSource> LicenseSources { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<Nationality> Nationalities { get; set; }

    // Additional lookup tables
    public DbSet<Classification> Classifications { get; set; }
    public DbSet<NotificationMethod> NotificationMethods { get; set; }
    public DbSet<GovernmentEntity> GovernmentEntities { get; set; }
    public DbSet<CaseType> CaseTypes { get; set; }

    #endregion

    #region Common DbSets

    public DbSet<Address> Addresses { get; set; }

    #endregion

    #region CaseRegistration DbSets

    public DbSet<CaseRegistrationRequest> CaseRegistrationRequests { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<RelatedCase> RelatedCases { get; set; }
    public DbSet<RequestClassification> RequestClassifications { get; set; }
    public DbSet<Plaintiff> Plaintiffs { get; set; }
    public DbSet<CaseRequestPlaintiff> CaseRequestPlaintiffs { get; set; }
    public DbSet<PlaintiffAttachment> PlaintiffAttachments { get; set; }
    public DbSet<Defendant> Defendants { get; set; }
    public DbSet<CaseRequestDefendant> CaseRequestDefendants { get; set; }
    public DbSet<Representative> Representatives { get; set; }
    public DbSet<RepresentativeAttachment> RepresentativeAttachments { get; set; }
    public DbSet<RequestAttachment> RequestAttachments { get; set; }

    // Additional info tables
    public DbSet<AdditionalInfo> AdditionalInfos { get; set; }
    public DbSet<AdditionalInfoManagementDecision> AdditionalInfoManagementDecisions { get; set; }
    public DbSet<AdditionalInfoServiceRights> AdditionalInfoServiceRights { get; set; }
    public DbSet<AdditionalInfoTrademark> AdditionalInfoTrademarks { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations using fluent API or separate configuration classes
        // This can be extended with IEntityTypeConfiguration implementations

        ConfigureUserEntity(modelBuilder);
        ConfigureIdentityEntities(modelBuilder);
        ConfigureLookupEntities(modelBuilder);
        ConfigureAdditionalLookupEntities(modelBuilder);
        ConfigureCommonEntities(modelBuilder);
        ConfigureCaseRegistrationEntities(modelBuilder);
        ConfigureAdditionalInfoEntities(modelBuilder);
        SeedRoles(modelBuilder);
        SeedLookupData(modelBuilder);
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

    /// <summary>
    /// Configures Identity entities (Role, UserRole, Court, Department, UserDepartment).
    /// </summary>
    private void ConfigureIdentityEntities(ModelBuilder modelBuilder)
    {
        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.NameAr)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        });

        // UserRole configuration (junction table)
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.RoleId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);
        });

        // Court configuration
        modelBuilder.Entity<Court>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.NameAr)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);
        });

        // Department configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.NameAr)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasOne(e => e.Court)
                .WithMany(c => c.Departments)
                .HasForeignKey(e => e.CourtId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);
        });

        // UserDepartment configuration (junction table)
        modelBuilder.Entity<UserDepartment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserDepartments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.UserDepartments)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.DepartmentId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);
        });
    }

    /// <summary>
    /// Seeds initial roles data.
    /// </summary>
    private void SeedRoles(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = 1,
                Name = "Clerk",
                NameAr = "كاتب",
                Description = "Can create, edit, and submit case registration requests",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = now,
                ModifiedDate = now
            },
            new Role
            {
                Id = 2,
                Name = "Reviewer",
                NameAr = "مدقق",
                Description = "Can review requests and take actions (register, reject, request completion)",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = now,
                ModifiedDate = now
            },
            new Role
            {
                Id = 3,
                Name = "Judge",
                NameAr = "قاضي",
                Description = "Can view cases on desk and approve or reject from judge desk",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = now,
                ModifiedDate = now
            },
            new Role
            {
                Id = 4,
                Name = "RegistrationEmployee",
                NameAr = "موظف قيد",
                Description = "Can create and register cases",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = now,
                ModifiedDate = now
            },
            new Role
            {
                Id = 5,
                Name = "StatusEmployee",
                NameAr = "موظف حالة",
                Description = "Can view and update case status",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = now,
                ModifiedDate = now
            },
            new Role
            {
                Id = 6,
                Name = "Admin",
                NameAr = "مدير",
                Description = "Full access to all case registration operations",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = now,
                ModifiedDate = now
            }
        );
    }

    /// <summary>
    /// Configures Lookup entities.
    /// </summary>
    private void ConfigureLookupEntities(ModelBuilder modelBuilder)
    {
        // PlaintiffType
        modelBuilder.Entity<PlaintiffType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // DefendantType
        modelBuilder.Entity<DefendantType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // RepresentativeType
        modelBuilder.Entity<RepresentativeType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // RequestStatus
        modelBuilder.Entity<RequestStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // IdentityType
        modelBuilder.Entity<IdentityType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // AttachmentType
        modelBuilder.Entity<AttachmentType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.AllowedExtensions).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // DataSource
        modelBuilder.Entity<DataSource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // Region
        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // City
        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Region)
                .WithMany(r => r.Cities)
                .HasForeignKey(e => e.RegionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // GovernmentAgency
        modelBuilder.Entity<GovernmentAgency>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // LicenseSource
        modelBuilder.Entity<LicenseSource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // Country
        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsoCode).HasMaxLength(3);
            entity.Property(e => e.PhoneCode).HasMaxLength(5);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // District
        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.City)
                .WithMany()
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Nationality
        modelBuilder.Entity<Nationality>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(3);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
    }

    /// <summary>
    /// Configures additional lookup entities (Classification, NotificationMethod, GovernmentEntity, CaseType).
    /// </summary>
    private void ConfigureAdditionalLookupEntities(ModelBuilder modelBuilder)
    {
        // Classification
        modelBuilder.Entity<Classification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Level1).HasMaxLength(100);
            entity.Property(e => e.Level2).HasMaxLength(100);
            entity.Property(e => e.Level3).HasMaxLength(100);
            entity.Property(e => e.Level4).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // NotificationMethod
        modelBuilder.Entity<NotificationMethod>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // GovernmentEntity
        modelBuilder.Entity<GovernmentEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // CaseType
        modelBuilder.Entity<CaseType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
    }

    /// <summary>
    /// Configures Common entities.
    /// </summary>
    private void ConfigureCommonEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BuildingNumber).HasMaxLength(20);
            entity.Property(e => e.StreetName).HasMaxLength(200);
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(10);
            entity.Property(e => e.AdditionalNumber).HasMaxLength(10);
            entity.Property(e => e.UnitNumber).HasMaxLength(20);
            entity.Property(e => e.AddressType).HasMaxLength(50);
            entity.Property(e => e.FullAddress).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
    }

    /// <summary>
    /// Configures CaseRegistration entities.
    /// </summary>
    private void ConfigureCaseRegistrationEntities(ModelBuilder modelBuilder)
    {
        // CaseRegistrationRequest
        modelBuilder.Entity<CaseRegistrationRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).HasMaxLength(4000);
            entity.Property(e => e.Evidence).HasMaxLength(4000);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.RejectionReason).HasMaxLength(2000);
            entity.Property(e => e.CaseNumber).HasMaxLength(50);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.RequestStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Court)
                .WithMany()
                .HasForeignKey(e => e.CourtId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.LastModifiedByUser)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Claim
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClaimText).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Request)
                .WithMany(r => r.Claims)
                .HasForeignKey(e => e.CaseRegistrationRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RelatedCase
        modelBuilder.Entity<RelatedCase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CaseNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Request)
                .WithMany(r => r.RelatedCases)
                .HasForeignKey(e => e.CaseRegistrationRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RequestClassification
        modelBuilder.Entity<RequestClassification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClassificationText).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Request)
                .WithMany(r => r.Classifications)
                .HasForeignKey(e => e.CaseRegistrationRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Plaintiff
        modelBuilder.Entity<Plaintiff>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IdentityNumber).HasMaxLength(20);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.FatherName).HasMaxLength(100);
            entity.Property(e => e.GrandfatherName).HasMaxLength(100);
            entity.Property(e => e.FamilyName).HasMaxLength(100);
            entity.Property(e => e.ClanName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.MobileNumber).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.CommercialRegNumber).HasMaxLength(20);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.AdditionalStatement).HasMaxLength(4000);
            entity.Property(e => e.LicenseNumber).HasMaxLength(50);
            entity.Property(e => e.LicenseSource).HasMaxLength(200);
            entity.Property(e => e.CourtDeedNumber).HasMaxLength(50);
            entity.Property(e => e.DeedSource).HasMaxLength(200);
            entity.Property(e => e.WaqfOversightType).HasMaxLength(50);
            entity.Property(e => e.Employer).HasMaxLength(200);
            entity.Property(e => e.Profession).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.PlaintiffType)
                .WithMany()
                .HasForeignKey(e => e.PlaintiffTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.IdentityType)
                .WithMany()
                .HasForeignKey(e => e.IdentityTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DataSource)
                .WithMany()
                .HasForeignKey(e => e.DataSourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.GovernmentAgency)
                .WithMany()
                .HasForeignKey(e => e.GovernmentAgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ResidenceAddress)
                .WithMany()
                .HasForeignKey(e => e.ResidenceAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WorkAddress)
                .WithMany()
                .HasForeignKey(e => e.WorkAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SelectedAddress)
                .WithMany()
                .HasForeignKey(e => e.SelectedAddressId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CaseRequestPlaintiff (junction table)
        modelBuilder.Entity<CaseRequestPlaintiff>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Request)
                .WithMany(r => r.CaseRequestPlaintiffs)
                .HasForeignKey(e => e.CaseRegistrationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Plaintiff)
                .WithMany(p => p.CaseRequestPlaintiffs)
                .HasForeignKey(e => e.PlaintiffId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CaseRegistrationRequestId, e.PlaintiffId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        });

        // PlaintiffAttachment
        modelBuilder.Entity<PlaintiffAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Plaintiff)
                .WithMany(p => p.Attachments)
                .HasForeignKey(e => e.PlaintiffId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AttachmentType)
                .WithMany()
                .HasForeignKey(e => e.AttachmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Defendant
        modelBuilder.Entity<Defendant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IdentityNumber).HasMaxLength(20);
            entity.Property(e => e.AddressText).HasMaxLength(500);
            entity.Property(e => e.CommercialRegNumber).HasMaxLength(20);
            entity.Property(e => e.Headquarters).HasMaxLength(200);
            entity.Property(e => e.AdditionalStatement).HasMaxLength(4000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.DefendantType)
                .WithMany()
                .HasForeignKey(e => e.DefendantTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.IdentityType)
                .WithMany()
                .HasForeignKey(e => e.IdentityTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DataSource)
                .WithMany()
                .HasForeignKey(e => e.DataSourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Address)
                .WithMany()
                .HasForeignKey(e => e.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.GovernmentAgency)
                .WithMany()
                .HasForeignKey(e => e.GovernmentAgencyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CaseRequestDefendant (junction table)
        modelBuilder.Entity<CaseRequestDefendant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Request)
                .WithMany(r => r.CaseRequestDefendants)
                .HasForeignKey(e => e.CaseRegistrationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Defendant)
                .WithMany(d => d.CaseRequestDefendants)
                .HasForeignKey(e => e.DefendantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CaseRegistrationRequestId, e.DefendantId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        });

        // Representative
        modelBuilder.Entity<Representative>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IdentityNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FatherName).HasMaxLength(100);
            entity.Property(e => e.GrandfatherName).HasMaxLength(100);
            entity.Property(e => e.FamilyName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClanName).HasMaxLength(100);
            entity.Property(e => e.MobileNumber).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.AuthorizationNumber).HasMaxLength(50);
            entity.Property(e => e.AuthorizationSource).HasMaxLength(200);
            entity.Property(e => e.AuthorizationSourceType).HasMaxLength(100);
            entity.Property(e => e.GuardianshipType).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Plaintiff)
                .WithMany(p => p.Representatives)
                .HasForeignKey(e => e.PlaintiffId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RepresentativeType)
                .WithMany()
                .HasForeignKey(e => e.RepresentativeTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.IdentityType)
                .WithMany()
                .HasForeignKey(e => e.IdentityTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DataSource)
                .WithMany()
                .HasForeignKey(e => e.DataSourceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RepresentativeAttachment
        modelBuilder.Entity<RepresentativeAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Representative)
                .WithMany(r => r.Attachments)
                .HasForeignKey(e => e.RepresentativeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AttachmentType)
                .WithMany()
                .HasForeignKey(e => e.AttachmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RequestAttachment
        modelBuilder.Entity<RequestAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.Request)
                .WithMany(r => r.Attachments)
                .HasForeignKey(e => e.CaseRegistrationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AttachmentType)
                .WithMany()
                .HasForeignKey(e => e.AttachmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Configures AdditionalInfo and related entities.
    /// </summary>
    private void ConfigureAdditionalInfoEntities(ModelBuilder modelBuilder)
    {
        // AdditionalInfo
        modelBuilder.Entity<AdditionalInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.CaseRegistrationRequest)
                .WithMany()
                .HasForeignKey(e => e.CaseRegistrationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ManagementDecision)
                .WithOne(m => m.AdditionalInfo)
                .HasForeignKey<AdditionalInfoManagementDecision>(m => m.AdditionalInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ServiceRights)
                .WithOne(s => s.AdditionalInfo)
                .HasForeignKey<AdditionalInfoServiceRights>(s => s.AdditionalInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Trademark)
                .WithOne(t => t.AdditionalInfo)
                .HasForeignKey<AdditionalInfoTrademark>(t => t.AdditionalInfoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AdditionalInfoManagementDecision
        modelBuilder.Entity<AdditionalInfoManagementDecision>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DecisionNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DecisionDate).IsRequired();
            entity.Property(e => e.NotificationDate).IsRequired();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.AdditionalInfo)
                .WithOne(a => a.ManagementDecision)
                .HasForeignKey<AdditionalInfoManagementDecision>(e => e.AdditionalInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.NotificationMethod)
                .WithMany()
                .HasForeignKey(e => e.NotificationMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.IssuingAuthority)
                .WithMany()
                .HasForeignKey(e => e.IssuingAuthorityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AdditionalInfoServiceRights
        modelBuilder.Entity<AdditionalInfoServiceRights>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.AdditionalInfo)
                .WithOne(a => a.ServiceRights)
                .HasForeignKey<AdditionalInfoServiceRights>(e => e.AdditionalInfoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AdditionalInfoTrademark
        modelBuilder.Entity<AdditionalInfoTrademark>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(e => e.AdditionalInfo)
                .WithOne(a => a.Trademark)
                .HasForeignKey<AdditionalInfoTrademark>(e => e.AdditionalInfoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Seeds lookup data.
    /// </summary>
    private void SeedLookupData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // PlaintiffTypes (8 types)
        modelBuilder.Entity<PlaintiffType>().HasData(
            new PlaintiffType { Id = 1, Name = "Individual", NameAr = "فرد", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 2, Name = "Company", NameAr = "شركة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 3, Name = "GovernmentAgency", NameAr = "جهة حكومية", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 4, Name = "Society", NameAr = "جمعية", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 5, Name = "Waqf", NameAr = "وقف", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 6, Name = "MinorOrIncapacitated", NameAr = "قاصر أو محجور عليه", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 7, Name = "Heir", NameAr = "وريث", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 8, Name = "BankruptEstate", NameAr = "تفليسة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );

        // DefendantTypes (6 types)
        modelBuilder.Entity<DefendantType>().HasData(
            new DefendantType { Id = 1, Name = "Individual", NameAr = "فرد", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new DefendantType { Id = 2, Name = "Company", NameAr = "شركة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new DefendantType { Id = 3, Name = "GovernmentAgency", NameAr = "جهة حكومية", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new DefendantType { Id = 4, Name = "Society", NameAr = "جمعية", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new DefendantType { Id = 5, Name = "Waqf", NameAr = "وقف", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new DefendantType { Id = 6, Name = "Unknown", NameAr = "مجهول", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );

        // RepresentativeTypes (9 types)
        modelBuilder.Entity<RepresentativeType>().HasData(
            new RepresentativeType { Id = 1, Name = "Agent", NameAr = "وكيل", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RepresentativeType { Id = 2, Name = "Guardian", NameAr = "ولي", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RepresentativeType { Id = 3, Name = "Custodian", NameAr = "وصي", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RepresentativeType { Id = 4, Name = "Executor", NameAr = "ناظر", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RepresentativeType { Id = 5, Name = "HeirRepresentative", NameAr = "ممثل الورثة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RepresentativeType { Id = 6, Name = "CompanyRepresentative", NameAr = "ممثل الشركة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RepresentativeType { Id = 7, Name = "AgencyRepresentative", NameAr = "ممثل الجهة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RepresentativeType { Id = 8, Name = "Trustee", NameAr = "أمين التفليسة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RepresentativeType { Id = 9, Name = "LegalRepresentative", NameAr = "ممثل نظامي", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );

        // RequestStatus (10 statuses)
        modelBuilder.Entity<RequestStatus>().HasData(
            new RequestStatus { Id = 1, Name = "Draft", NameAr = "مسودة", DisplayOrder = 1, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RequestStatus { Id = 2, Name = "New", NameAr = "جديد", DisplayOrder = 2, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RequestStatus { Id = 3, Name = "UnderReview", NameAr = "قيد المراجعة", DisplayOrder = 3, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RequestStatus { Id = 4, Name = "Registered", NameAr = "مقيد", DisplayOrder = 4, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RequestStatus { Id = 5, Name = "Rejected", NameAr = "مرفوض", DisplayOrder = 5, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RequestStatus { Id = 6, Name = "PendingCompletion", NameAr = "بانتظار الاستكمال", DisplayOrder = 6, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RequestStatus { Id = 7, Name = "OnJudgeDesk", NameAr = "على مكتب القاضي", DisplayOrder = 7, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RequestStatus { Id = 8, Name = "Completed", NameAr = "مكتمل", DisplayOrder = 8, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RequestStatus { Id = 9, Name = "AutoRejected", NameAr = "مرفوض تلقائياً", DisplayOrder = 9, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new RequestStatus { Id = 10, Name = "Cancelled", NameAr = "ملغي", DisplayOrder = 10, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );

        // IdentityTypes (3 types)
        modelBuilder.Entity<IdentityType>().HasData(
            new IdentityType { Id = 1, Name = "NationalID", NameAr = "هوية وطنية", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new IdentityType { Id = 2, Name = "ResidentID", NameAr = "إقامة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new IdentityType { Id = 3, Name = "Passport", NameAr = "جواز سفر", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );

        // DataSource (2 types)
        modelBuilder.Entity<DataSource>().HasData(
            new DataSource { Id = 1, Name = "FromAbsher", NameAr = "من أبشر", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new DataSource { Id = 2, Name = "FromUser", NameAr = "من المستخدم", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );

        // AttachmentTypes (basic types)
        modelBuilder.Entity<AttachmentType>().HasData(
            new AttachmentType { Id = 1, Name = "IdentityCopy", NameAr = "صورة الهوية", IsMandatory = true, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new AttachmentType { Id = 2, Name = "PowerOfAttorney", NameAr = "صك الوكالة", IsMandatory = false, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new AttachmentType { Id = 3, Name = "CommercialRegistration", NameAr = "السجل التجاري", IsMandatory = false, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new AttachmentType { Id = 4, Name = "License", NameAr = "الترخيص", IsMandatory = false, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new AttachmentType { Id = 5, Name = "Deed", NameAr = "الصك", IsMandatory = false, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new AttachmentType { Id = 6, Name = "SupportingDocument", NameAr = "مستند داعم", IsMandatory = false, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );

        // Regions (13 Saudi regions)
        modelBuilder.Entity<Region>().HasData(
            new Region { Id = 1, Name = "Riyadh", NameAr = "الرياض", Code = "01", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Region { Id = 2, Name = "Makkah", NameAr = "مكة المكرمة", Code = "02", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Region { Id = 3, Name = "Madinah", NameAr = "المدينة المنورة", Code = "03", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Region { Id = 4, Name = "Qassim", NameAr = "القصيم", Code = "04", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Region { Id = 5, Name = "Eastern", NameAr = "الشرقية", Code = "05", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Region { Id = 6, Name = "Asir", NameAr = "عسير", Code = "06", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Region { Id = 7, Name = "Tabuk", NameAr = "تبوك", Code = "07", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Region { Id = 8, Name = "Hail", NameAr = "حائل", Code = "08", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Region { Id = 9, Name = "NorthernBorders", NameAr = "الحدود الشمالية", Code = "09", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Region { Id = 10, Name = "Jazan", NameAr = "جازان", Code = "10", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Region { Id = 11, Name = "Najran", NameAr = "نجران", Code = "11", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Region { Id = 12, Name = "Bahah", NameAr = "الباحة", Code = "12", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Region { Id = 13, Name = "Jawf", NameAr = "الجوف", Code = "13", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );

        // Cities (major cities)
        modelBuilder.Entity<City>().HasData(
            new City { Id = 1, Name = "Riyadh", NameAr = "الرياض", RegionId = 1, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new City { Id = 2, Name = "Jeddah", NameAr = "جدة", RegionId = 2, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new City { Id = 3, Name = "Makkah", NameAr = "مكة المكرمة", RegionId = 2, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new City { Id = 4, Name = "Madinah", NameAr = "المدينة المنورة", RegionId = 3, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new City { Id = 5, Name = "Dammam", NameAr = "الدمام", RegionId = 5, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new City { Id = 6, Name = "Khobar", NameAr = "الخبر", RegionId = 5, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new City { Id = 7, Name = "Dhahran", NameAr = "الظهران", RegionId = 5, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new City { Id = 8, Name = "Taif", NameAr = "الطائف", RegionId = 2, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new City { Id = 9, Name = "Buraidah", NameAr = "بريدة", RegionId = 4, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new City { Id = 10, Name = "Tabuk", NameAr = "تبوك", RegionId = 7, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new City { Id = 11, Name = "Abha", NameAr = "أبها", RegionId = 6, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new City { Id = 12, Name = "Hail", NameAr = "حائل", RegionId = 8, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new City { Id = 13, Name = "Najran", NameAr = "نجران", RegionId = 11, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );

        // Classifications (25 hierarchical classifications)
        modelBuilder.Entity<Classification>().HasData(
            new Classification { Id = 7, Name = "RealEstateSaleContract", NameAr = "عقد بيع عقار", Description = "Sale contract for real estate", Level1 = "عقود", Level2 = "عقود مدنية", Level3 = "عقود البيع", Level4 = "عقد بيع عقار", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 8, Name = "ResidentialLeaseContract", NameAr = "عقد إيجار سكني", Description = "Lease contract for residential property", Level1 = "عقود", Level2 = "عقود مدنية", Level3 = "عقود الإيجار", Level4 = "عقد إيجار سكني", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 9, Name = "CompanyFormationContract", NameAr = "عقد تأسيس شركة", Description = "Partnership formation agreement", Level1 = "عقود", Level2 = "عقود تجارية", Level3 = "عقود الشركات", Level4 = "عقد تأسيس شركة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 10, Name = "FixedTermEmploymentContract", NameAr = "عقد عمل محدد المدة", Description = "Fixed term employment agreement", Level1 = "عقود", Level2 = "عقود تجارية", Level3 = "عقود العمل", Level4 = "عقد عمل محدد المدة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 11, Name = "RealEstateOwnershipDispute", NameAr = "نزاع ملكية عقار", Description = "Property ownership dispute", Level1 = "دعاوى مدنية", Level2 = "دعاوى الملكية", Level3 = "النزاعات العقارية", Level4 = "نزاع ملكية عقار", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 12, Name = "FinancialCompensation", NameAr = "تعويض عن ضرر مالي", Description = "Claim for financial damages", Level1 = "دعاوى مدنية", Level2 = "دعاوى التعويض", Level3 = "التعويض المالي", Level4 = "تعويض عن ضرر مادي", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 13, Name = "CommercialDebtCollection", NameAr = "دعوى تحصيل دين تجاري", Description = "Collection of commercial debt", Level1 = "دعاوى مدنية", Level2 = "دعاوى الديون", Level3 = "تحصيل الديون", Level4 = "دعوى تحصيل دين تجاري", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 14, Name = "PartnershipDividendDispute", NameAr = "نزاع توزيع أرباح", Description = "Partnership profit distribution dispute", Level1 = "دعاوى تجارية", Level2 = "منازعات الشركات", Level3 = "النزاعات بين الشركاء", Level4 = "نزاع توزيع أرباح", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 15, Name = "BankruptcyDeclaration", NameAr = "طلب إشهار إفلاس", Description = "Bankruptcy filing request", Level1 = "دعاوى تجارية", Level2 = "الإفلاس والتصفية", Level3 = "إجراءات الإفلاس", Level4 = "طلب إشهار إفلاس", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 16, Name = "CommercialContractDispute", NameAr = "نزاع عقد تجاري", Description = "Commercial contract dispute", Level1 = "دعاوى تجارية", Level2 = "منازعات العقود", Level3 = "نزاعات التنفيذ", Level4 = "نزاع عقد تجاري", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 17, Name = "UnpaidWagesClaim", NameAr = "مطالبة بأجور متأخرة", Description = "Claim for unpaid wages", Level1 = "دعاوى عمالية", Level2 = "حقوق العمال", Level3 = "مستحقات مالية", Level4 = "مطالبة بأجور متأخرة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 18, Name = "UnfairTermination", NameAr = "دعوى فصل تعسفي", Description = "Unfair termination lawsuit", Level1 = "دعاوى عمالية", Level2 = "إنهاء الخدمة", Level3 = "الفصل التعسفي", Level4 = "دعوى فصل تعسفي", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 19, Name = "WorkplaceInjury", NameAr = "دعوى إصابة عمل", Description = "Workplace injury claim", Level1 = "دعاوى عمالية", Level2 = "الحقوق والواجبات", Level3 = "الحماية والأمان", Level4 = "دعوى إصابة عمل", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 20, Name = "MarriageCase", NameAr = "دعوى زواج", Description = "Marriage-related case", Level1 = "دعاوى أحوال شخصية", Level2 = "دعاوى الزواج", Level3 = "", Level4 = "", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 21, Name = "DivorceCase", NameAr = "دعوى طلاق", Description = "Divorce case", Level1 = "دعاوى أحوال شخصية", Level2 = "دعاوى الطلاق", Level3 = "", Level4 = "", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 22, Name = "CommercialLeaseContract", NameAr = "عقد إيجار تجاري", Description = "Commercial property lease", Level1 = "عقود", Level2 = "عقود مدنية", Level3 = "عقود الإيجار", Level4 = "عقد إيجار تجاري", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 23, Name = "GiftContract", NameAr = "عقد هبة", Description = "Gift agreement", Level1 = "عقود", Level2 = "عقود مدنية", Level3 = "عقود الملكية", Level4 = "عقد هبة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 24, Name = "LoanContract", NameAr = "عقد قرض", Description = "Loan agreement", Level1 = "عقود", Level2 = "عقود مدنية", Level3 = "عقود الالتزام", Level4 = "عقد قرض", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 25, Name = "PersonalInjuryCompensation", NameAr = "تعويض عن إصابة شخصية", Description = "Personal injury compensation", Level1 = "دعاوى مدنية", Level2 = "دعاوى التعويض", Level3 = "التعويض الشخصي", Level4 = "تعويض عن إصابة شخصية", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 26, Name = "PropertyDamageCompensation", NameAr = "تعويض عن تلف الملكية", Description = "Property damage compensation", Level1 = "دعاوى مدنية", Level2 = "دعاوى التعويض", Level3 = "تعويض عن الأضرار", Level4 = "تعويض عن تلف الملكية", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 27, Name = "PaymentDefaultCase", NameAr = "دعوى عدم السداد", Description = "Non-payment default case", Level1 = "دعاوى مدنية", Level2 = "دعاوى الديون", Level3 = "ديون المستهلكين", Level4 = "دعوى عدم السداد", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 28, Name = "IntellectualPropertyDispute", NameAr = "نزاع الملكية الفكرية", Description = "Intellectual property dispute", Level1 = "دعاوى تجارية", Level2 = "حقوق الملكية", Level3 = "براءات الاختراع", Level4 = "نزاع براءة اختراع", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 29, Name = "CompetitionLawViolation", NameAr = "انتهاك قانون المنافسة", Description = "Competition law violation", Level1 = "دعاوى تجارية", Level2 = "الممارسات غير العادلة", Level3 = "الاحتكار والتنافس", Level4 = "انتهاك قانون المنافسة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Classification { Id = 30, Name = "WorkplaceHarassment", NameAr = "دعوى التحرش في العمل", Description = "Workplace harassment claim", Level1 = "دعاوى عمالية", Level2 = "حقوق العمال", Level3 = "الحقوق الشخصية", Level4 = "دعوى التحرش في العمل", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );
    }
}
