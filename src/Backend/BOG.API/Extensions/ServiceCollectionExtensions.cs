using BOG.BL.Interfaces;
using BOG.BL.Services;
using BOG.DAL.Interfaces;
using BOG.DAL.Repositories;
using BOG.DbModel;
using BOG.Integration.Interfaces;
using BOG.Integration.Services;
using Microsoft.EntityFrameworkCore;

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
            options.UseSqlServer(connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly("BOG.DbModel"))
        );

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

        // Identity repositories
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ICourtRepository, CourtRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();

        // Plaintiff & Representative repositories
        services.AddScoped<IPlaintiffRepository, PlaintiffRepository>();
        services.AddScoped<IRepresentativeRepository, RepresentativeRepository>();
        services.AddScoped<IPlaintiffAttachmentRepository, PlaintiffAttachmentRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<ICaseRequestPlaintiffRepository, CaseRequestPlaintiffRepository>();

        return services;
    }

    /// <summary>
    /// Registers business logic services.
    /// Follows Dependency Inversion Principle - depends on service interfaces.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
    {
        // Feature-specific services
        services.AddScoped<IUserBL, UserBL>();

        // Identity and authorization services
        services.AddScoped<IRoleBL, RoleBL>();
        services.AddScoped<IAuthorizationBL, AuthorizationBL>();

        // Plaintiff & Representative services
        services.AddScoped<IPlaintiffBL, PlaintiffBL>();
        services.AddScoped<IRepresentativeBL, RepresentativeBL>();
        services.AddScoped<IPlaintiffAttachmentBL, PlaintiffAttachmentBL>();

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
            .AddBusinessLogicServices()
            .AddIntegrationServices();

        return services;
    }
}
