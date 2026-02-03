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
    public DbSet<RequestAttachment> RequestAttachments { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations using fluent API or separate configuration classes
        // This can be extended with IEntityTypeConfiguration implementations

        ConfigureUserEntity(modelBuilder);
        ConfigureIdentityEntities(modelBuilder);
        ConfigureLookupEntities(modelBuilder);
        ConfigureCommonEntities(modelBuilder);
        ConfigureCaseRegistrationEntities(modelBuilder);
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

            // Navigation properties for Region and City
            entity.HasOne(e => e.Region)
                .WithMany()
                .HasForeignKey(e => e.RegionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.City_)
                .WithMany()
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.Property(e => e.Headquarters).HasMaxLength(200);
            entity.Property(e => e.LicenseNumber).HasMaxLength(10);
            entity.Property(e => e.NGOName).HasMaxLength(200);
            entity.Property(e => e.CourtDeedNumber).HasMaxLength(10);
            entity.Property(e => e.WaqfName).HasMaxLength(200);
            entity.Property(e => e.DeedSource).HasMaxLength(100);
            entity.Property(e => e.WaqfOversightType).HasMaxLength(20);
            entity.Property(e => e.WaqfAgencyName).HasMaxLength(200);
            entity.Property(e => e.WaqfDescription).HasMaxLength(200);
            entity.Property(e => e.Employer).HasMaxLength(200);
            entity.Property(e => e.Profession).HasMaxLength(200);
            entity.Property(e => e.UnregisteredCompanyAddress).HasMaxLength(500);
            entity.Property(e => e.UnregisteredCompanyCity).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
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

            entity.HasOne(e => e.LicenseSource)
                .WithMany()
                .HasForeignKey(e => e.LicenseSourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Country)
                .WithMany()
                .HasForeignKey(e => e.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ResidenceAddress)
                .WithMany()
                .HasForeignKey(e => e.ResidenceAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WorkAddress)
                .WithMany()
                .HasForeignKey(e => e.WorkAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BusinessAddress)
                .WithMany()
                .HasForeignKey(e => e.BusinessAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CompanyAddress)
                .WithMany()
                .HasForeignKey(e => e.CompanyAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.NGOAddress)
                .WithMany()
                .HasForeignKey(e => e.NGOAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WaqfAddress)
                .WithMany()
                .HasForeignKey(e => e.WaqfAddressId)
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
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.Headquarters).HasMaxLength(200);
            entity.Property(e => e.AdditionalStatement).HasMaxLength(4000);
            // Registered Company (Type 2) fields
            entity.Property(e => e.RegCompanyDistrict).HasMaxLength(100);
            entity.Property(e => e.RegCompanyStreet).HasMaxLength(200);
            entity.Property(e => e.RegCompanyBuildingNumber).HasMaxLength(4);
            entity.Property(e => e.RegCompanyUnitNumber).HasMaxLength(20);
            entity.Property(e => e.RegCompanyPostalCode).HasMaxLength(5);
            entity.Property(e => e.RegCompanyAdditionalCode).HasMaxLength(4);
            // Unregistered Company (Type 4) fields
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            // Waqf (Type 7) fields
            entity.Property(e => e.WaqfName).HasMaxLength(200);
            entity.Property(e => e.CourtDeedNumber).HasMaxLength(10);
            entity.Property(e => e.DeedSource).HasMaxLength(100);
            entity.Property(e => e.WaqfAgencyName).HasMaxLength(200);
            entity.Property(e => e.WaqfDistrict).HasMaxLength(100);
            entity.Property(e => e.WaqfStreet).HasMaxLength(200);
            entity.Property(e => e.WaqfBuildingNumber).HasMaxLength(4);
            entity.Property(e => e.WaqfUnitNumber).HasMaxLength(20);
            entity.Property(e => e.WaqfPostalCode).HasMaxLength(5);
            entity.Property(e => e.WaqfAdditionalCode).HasMaxLength(4);
            entity.Property(e => e.WaqfAddressDescription).HasMaxLength(500);
            // NGO (Type 6) fields
            entity.Property(e => e.LicenseNumber).HasMaxLength(10);
            entity.Property(e => e.NGOName).HasMaxLength(200);
            entity.Property(e => e.NGODistrict).HasMaxLength(100);
            entity.Property(e => e.NGOStreet).HasMaxLength(200);
            entity.Property(e => e.NGOBuildingNumber).HasMaxLength(4);
            entity.Property(e => e.NGOUnitNumber).HasMaxLength(20);
            entity.Property(e => e.NGOPostalCode).HasMaxLength(5);
            entity.Property(e => e.NGOAdditionalCode).HasMaxLength(4);
            // Individual (Type 1) fields
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.FatherName).HasMaxLength(100);
            entity.Property(e => e.GrandfatherName).HasMaxLength(100);
            entity.Property(e => e.TribeName).HasMaxLength(100);
            entity.Property(e => e.FamilyName).HasMaxLength(100);
            entity.Property(e => e.MobileNumber).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IndDistrict).HasMaxLength(100);
            entity.Property(e => e.IndStreet).HasMaxLength(200);
            entity.Property(e => e.IndBuildingNumber).HasMaxLength(4);
            entity.Property(e => e.IndUnitNumber).HasMaxLength(4);
            entity.Property(e => e.IndPostalCode).HasMaxLength(5);
            entity.Property(e => e.IndAdditionalCode).HasMaxLength(4);
            entity.Property(e => e.Employer).HasMaxLength(200);
            entity.Property(e => e.Occupation).HasMaxLength(200);
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

            // Registered Company (Type 2) navigation properties
            entity.HasOne(e => e.RegCompanyRegion)
                .WithMany()
                .HasForeignKey(e => e.RegCompanyRegionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RegCompanyCity)
                .WithMany()
                .HasForeignKey(e => e.RegCompanyCityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Country)
                .WithMany()
                .HasForeignKey(e => e.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Waqf (Type 7) navigation properties
            entity.HasOne(e => e.WaqfRegion)
                .WithMany()
                .HasForeignKey(e => e.WaqfRegionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WaqfCity)
                .WithMany()
                .HasForeignKey(e => e.WaqfCityId)
                .OnDelete(DeleteBehavior.Restrict);

            // NGO (Type 6) navigation properties
            entity.HasOne(e => e.LicenseSource)
                .WithMany()
                .HasForeignKey(e => e.LicenseSourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.NGORegion)
                .WithMany()
                .HasForeignKey(e => e.NGORegionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.NGOCity)
                .WithMany()
                .HasForeignKey(e => e.NGOCityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.IndRegion)
                .WithMany()
                .HasForeignKey(e => e.IndRegionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.IndCity)
                .WithMany()
                .HasForeignKey(e => e.IndCityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Work address (Individual Type 1, Private employment)
            entity.HasOne(e => e.WorkRegion)
                .WithMany()
                .HasForeignKey(e => e.WorkRegionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WorkCity)
                .WithMany()
                .HasForeignKey(e => e.WorkCityId)
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
    /// Seeds lookup data.
    /// </summary>
    private void SeedLookupData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // PlaintiffTypes (8 types per SRS Section 1.3)
        modelBuilder.Entity<PlaintiffType>().HasData(
            new PlaintiffType { Id = 1, Name = "Individual", NameAr = "فرد", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 2, Name = "IndividualWithoutId", NameAr = "فرد بدون هوية", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 3, Name = "BusinessOwner", NameAr = "صاحب مؤسسة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 4, Name = "RegisteredCompany", NameAr = "شركة مسجلة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 5, Name = "UnregisteredCompany", NameAr = "شركة غير مسجلة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 6, Name = "GovernmentAgency", NameAr = "جهة حكومية", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 7, Name = "Society", NameAr = "جمعية/مؤسسة أهلية", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new PlaintiffType { Id = 8, Name = "Waqf", NameAr = "وقف", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );

        // DefendantTypes (7 types) - matching defendant-user-stories-plan.html
        modelBuilder.Entity<DefendantType>().HasData(
            new DefendantType { Id = 1, Name = "Individual", NameAr = "فرد", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new DefendantType { Id = 2, Name = "RegisteredCompany", NameAr = "شركة مسجلة في المملكة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new DefendantType { Id = 3, Name = "GovernmentAgency", NameAr = "جهة حكومية", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new DefendantType { Id = 4, Name = "UnregisteredCompany", NameAr = "شركة غير مسجلة في المملكة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new DefendantType { Id = 5, Name = "BusinessOwner", NameAr = "صاحب مؤسسة", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new DefendantType { Id = 6, Name = "NGO", NameAr = "جمعية/مؤسسة أهلية", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new DefendantType { Id = 7, Name = "Waqf", NameAr = "وقف", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
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

        // LicenseSources (for NGO - مصدر الترخيص)
        modelBuilder.Entity<LicenseSource>().HasData(
            new LicenseSource { Id = 1, Name = "MinistryOfLabor", NameAr = "وزارة العمل والتنمية الاجتماعية", Code = "MOL", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new LicenseSource { Id = 2, Name = "MinistryOfCommerce", NameAr = "وزارة التجارة", Code = "MOC", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new LicenseSource { Id = 3, Name = "GeneralAuthorityForAwqaf", NameAr = "الهيئة العامة للأوقاف", Code = "GAA", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new LicenseSource { Id = 4, Name = "SaudiCentralBank", NameAr = "البنك المركزي السعودي", Code = "SAMA", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new LicenseSource { Id = 5, Name = "CapitalMarketAuthority", NameAr = "هيئة السوق المالية", Code = "CMA", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );

        // Countries (for unregistered foreign companies)
        modelBuilder.Entity<Country>().HasData(
            new Country { Id = 1, Name = "Saudi Arabia", NameAr = "المملكة العربية السعودية", IsoCode = "SA", PhoneCode = "966", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 2, Name = "United Arab Emirates", NameAr = "الإمارات العربية المتحدة", IsoCode = "AE", PhoneCode = "971", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 3, Name = "Kuwait", NameAr = "الكويت", IsoCode = "KW", PhoneCode = "965", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 4, Name = "Bahrain", NameAr = "البحرين", IsoCode = "BH", PhoneCode = "973", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 5, Name = "Qatar", NameAr = "قطر", IsoCode = "QA", PhoneCode = "974", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 6, Name = "Oman", NameAr = "عُمان", IsoCode = "OM", PhoneCode = "968", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 7, Name = "Egypt", NameAr = "مصر", IsoCode = "EG", PhoneCode = "20", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 8, Name = "Jordan", NameAr = "الأردن", IsoCode = "JO", PhoneCode = "962", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 9, Name = "Lebanon", NameAr = "لبنان", IsoCode = "LB", PhoneCode = "961", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 10, Name = "Syria", NameAr = "سوريا", IsoCode = "SY", PhoneCode = "963", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 11, Name = "Iraq", NameAr = "العراق", IsoCode = "IQ", PhoneCode = "964", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 12, Name = "Yemen", NameAr = "اليمن", IsoCode = "YE", PhoneCode = "967", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 13, Name = "United States", NameAr = "الولايات المتحدة", IsoCode = "US", PhoneCode = "1", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 14, Name = "United Kingdom", NameAr = "المملكة المتحدة", IsoCode = "GB", PhoneCode = "44", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 15, Name = "France", NameAr = "فرنسا", IsoCode = "FR", PhoneCode = "33", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 16, Name = "Germany", NameAr = "ألمانيا", IsoCode = "DE", PhoneCode = "49", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 17, Name = "India", NameAr = "الهند", IsoCode = "IN", PhoneCode = "91", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 18, Name = "Pakistan", NameAr = "باكستان", IsoCode = "PK", PhoneCode = "92", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 19, Name = "China", NameAr = "الصين", IsoCode = "CN", PhoneCode = "86", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new Country { Id = 20, Name = "Japan", NameAr = "اليابان", IsoCode = "JP", PhoneCode = "81", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );

        // Districts (sample districts for major cities)
        modelBuilder.Entity<District>().HasData(
            // Riyadh districts
            new District { Id = 1, Name = "Al Olaya", NameAr = "العليا", CityId = 1, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new District { Id = 2, Name = "Al Malaz", NameAr = "الملز", CityId = 1, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new District { Id = 3, Name = "Al Naseem", NameAr = "النسيم", CityId = 1, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new District { Id = 4, Name = "Al Wurud", NameAr = "الورود", CityId = 1, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new District { Id = 5, Name = "Al Sulimaniyah", NameAr = "السليمانية", CityId = 1, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            // Jeddah districts
            new District { Id = 6, Name = "Al Rawdah", NameAr = "الروضة", CityId = 2, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new District { Id = 7, Name = "Al Hamra", NameAr = "الحمراء", CityId = 2, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new District { Id = 8, Name = "Al Shati", NameAr = "الشاطئ", CityId = 2, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            // Dammam districts
            new District { Id = 9, Name = "Al Faisaliyah", NameAr = "الفيصلية", CityId = 5, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new District { Id = 10, Name = "Al Anoud", NameAr = "العنود", CityId = 5, IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );

        // GovernmentAgencies (الجهات الحكومية)
        modelBuilder.Entity<GovernmentAgency>().HasData(
            new GovernmentAgency { Id = 1, Name = "Ministry of Justice", NameAr = "وزارة العدل", Code = "MOJ", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 2, Name = "Ministry of Interior", NameAr = "وزارة الداخلية", Code = "MOI", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 3, Name = "Ministry of Finance", NameAr = "وزارة المالية", Code = "MOF", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 4, Name = "Ministry of Health", NameAr = "وزارة الصحة", Code = "MOH", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 5, Name = "Ministry of Education", NameAr = "وزارة التعليم", Code = "MOE", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 6, Name = "Ministry of Commerce", NameAr = "وزارة التجارة", Code = "MOC", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 7, Name = "Ministry of Human Resources", NameAr = "وزارة الموارد البشرية والتنمية الاجتماعية", Code = "HRSD", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 8, Name = "Ministry of Transport", NameAr = "وزارة النقل والخدمات اللوجستية", Code = "MOT", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 9, Name = "Ministry of Municipal Affairs", NameAr = "وزارة الشؤون البلدية والقروية والإسكان", Code = "MOMRA", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 10, Name = "Ministry of Environment", NameAr = "وزارة البيئة والمياه والزراعة", Code = "MEWA", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 11, Name = "General Authority of Zakat and Tax", NameAr = "هيئة الزكاة والضريبة والجمارك", Code = "ZATCA", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 12, Name = "General Organization for Social Insurance", NameAr = "المؤسسة العامة للتأمينات الاجتماعية", Code = "GOSI", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 13, Name = "Saudi Central Bank", NameAr = "البنك المركزي السعودي", Code = "SAMA", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 14, Name = "Capital Market Authority", NameAr = "هيئة السوق المالية", Code = "CMA", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now },
            new GovernmentAgency { Id = 15, Name = "Royal Court", NameAr = "الديوان الملكي", Code = "RC", IsActive = true, IsDeleted = false, CreatedDate = now, ModifiedDate = now }
        );
    }
}
