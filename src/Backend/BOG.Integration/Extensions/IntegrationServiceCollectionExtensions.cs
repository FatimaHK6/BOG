using BOG.Integration.Configuration;
using BOG.Integration.Interfaces;
using BOG.Integration.Services.CaseManagement;
using BOG.Integration.Services.Email;
using BOG.Integration.Services.Sms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BOG.Integration.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to configure integration services.
/// Follows Single Responsibility Principle - manages only integration service registration.
/// Follows Open-Closed Principle - can be extended with new service registrations.
/// </summary>
public static class IntegrationServiceCollectionExtensions
{
    /// <summary>
    /// Registers integration services (SMS, Email, Case Management).
    /// Uses mock implementations by default, configurable via settings.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddIntegrationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register configuration options
        services.Configure<SmsSettings>(configuration.GetSection("Integration:Sms"));
        services.Configure<EmailSettings>(configuration.GetSection("Integration:Email"));
        services.Configure<CaseManagementSettings>(configuration.GetSection("Integration:CaseManagement"));

        // Register SMS service
        var smsMockEnabled = configuration.GetValue<bool>("Integration:Sms:UseMock", true);
        services.AddScoped<ISmsService, MockSmsService>();
        LogServiceRegistration("SMS", "MockSmsService", smsMockEnabled);

        // Register Email service
        var emailMockEnabled = configuration.GetValue<bool>("Integration:Email:UseMock", true);
        services.AddScoped<IEmailService, MockEmailService>();
        LogServiceRegistration("Email", "MockEmailService", emailMockEnabled);

        // Register Case Management service
        var caseManagementMockEnabled = configuration.GetValue<bool>("Integration:CaseManagement:UseMock", true);
        services.AddScoped<ICaseManagementService, MockCaseManagementService>();
        LogServiceRegistration("CaseManagement", "MockCaseManagementService", caseManagementMockEnabled);

        return services;
    }

    /// <summary>
    /// Helper to log service registration status.
    /// </summary>
    private static void LogServiceRegistration(string serviceName, string implementationName, bool isMock)
    {
        var mode = isMock ? "MOCK" : "REAL";
        Console.WriteLine($"[Integration] {serviceName} service registered as {implementationName} ({mode} mode)");
    }
}
