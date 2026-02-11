using BOG.BL.Configuration;
using BOG.BL.Interfaces;
using BOG.BL.Interfaces.CaseRegistration;
using BOG.BL.Services;
using BOG.BL.Services.CaseRegistration;
using BOG.BL.Services.FileStorage;
using BOG.DAL.Interfaces;
using BOG.DAL.Repositories;
using BOG.DbModel;
using BOG.DTO.Plaintiff;
using BOG.Integration.Interfaces;
using BOG.Integration.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Configuration;

namespace BOG.API.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to configure dependency injection.
/// Follows Single Responsibility Principle - manages only DI registration.
/// Follows Open-Closed Principle - can be extended with new registration methods.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the database context and configures EF Core.
    /// Follows Dependency Inversion Principle - configures abstraction (DbContext interface).
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplicationDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var provider = configuration["Database:Provider"] ?? "SqlServer";

            if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase("BOG_Development");
            }
            else
            {
                options.UseSqlServer(connectionString,
                    sqlOptions => sqlOptions
                        .MigrationsAssembly("BOG.DbModel")
                        .EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null));
            }
        });

        // Register ApplicationDbContext as DbContext for dependency injection
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    /// <summary>
    /// Registers the Unit of Work pattern.
    /// Follows Dependency Inversion Principle - depends on IUnitOfWork abstraction.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    /// <summary>
    /// Registers repository services.
    /// Follows Dependency Inversion Principle - depends on IRepository<T> abstraction.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Generic repository registration
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Feature-specific repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICaseRegistrationRequestRepository, CaseRegistrationRequestRepository>();

        // Case registration related repositories
        services.AddScoped<IDefendantRepository, DefendantRepository>();
        services.AddScoped<IPlaintiffRepository, PlaintiffRepository>();
        services.AddScoped<IPlaintiffAttachmentRepository, PlaintiffAttachmentRepository>();
        services.AddScoped<ICaseRequestDefendantRepository, CaseRequestDefendantRepository>();
        services.AddScoped<ICaseRequestPlaintiffRepository, CaseRequestPlaintiffRepository>();
        services.AddScoped<IRepresentativeRepository, RepresentativeRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IRequestAttachmentRepository, RequestAttachmentRepository>();
        services.AddScoped<IAdditionalInfoRepository, AdditionalInfoRepository>();
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IRelatedCaseRepository, RelatedCaseRepository>();

        // Identity repositories
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ICourtRepository, CourtRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();

        return services;
    }

    /// <summary>
    /// Registers business logic services.
    /// Follows Dependency Inversion Principle - depends on service interfaces.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddBusinessLogicServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure FileStorageSettings from appsettings
        services.Configure<FileStorageSettings>(
            configuration.GetSection("FileStorage") ?? new ConfigurationBuilder().Build().GetSection("FileStorage"));

        // Feature-specific services
        services.AddScoped<IUserBL, UserBL>();
        services.AddScoped<ICaseRegistrationBL, CaseRegistrationBL>();
        services.AddScoped<ICaseRegistrationRequestBL, CaseRegistrationRequestBL>();
        services.AddScoped<IRequestActionBL, RequestActionBL>();

        // Case registration related services
        services.AddScoped<IDefendantBL, DefendantBL>();
        services.AddScoped<IPlaintiffBL, PlaintiffBL>();
        services.AddScoped<IPlaintiffAttachmentBL, PlaintiffAttachmentBL>();
        services.AddScoped<IRequestAttachmentBL, RequestAttachmentBL>();
        services.AddScoped<IAdditionalInfoBL, AdditionalInfoBL>();
        services.AddScoped<IClaimBL, ClaimBL>();
        services.AddScoped<IRelatedCaseBL, RelatedCaseBL>();

        // File storage service
        services.AddScoped<IFileStorageService, FileStorageService>();

        // Identity and authorization services
        services.AddScoped<IRoleBL, RoleBL>();
        services.AddScoped<IAuthorizationBL, AuthorizationBL>();

        return services;
    }

    /// <summary>
    /// Registers integration services (external system integrations).
    /// Follows Dependency Inversion Principle - depends on service interfaces.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
    {
        // Absher integration (using mock for development)
        services.AddScoped<IAbsherService, AbsherMockService>();

        return services;
    }

    /// <summary>
    /// Registers FluentValidation services.
    /// Registers all validators from the DTO assembly for dependency injection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        // Register all validators from the DTO assembly
        services.AddValidatorsFromAssemblyContaining<PlaintiffCreateDTOValidator>();

        return services;
    }

    /// <summary>
    /// Registers all application services in one convenient method.
    /// Follows Open-Closed Principle - can be extended with new service registrations.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddApplicationDbContext(configuration)
            .AddUnitOfWork()
            .AddRepositories()
            .AddBusinessLogicServices(configuration)
            .AddIntegrationServices()
            .AddFluentValidation();

        return services;
    }
}
