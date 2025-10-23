// src\UserHealthService\UserHealthService.Infrastructure\Data\SeedAdmin.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserHealthService.Application.DTOs.Auth;
using UserHealthService.Application.Interfaces;
using UserHealthService.Domain.Enums;

namespace UserHealthService.Infrastructure.Data
{
    public static class SeedAdmin
    {
       public static async Task EnsureSuperAdminAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("SeedAdmin");


    try
    {
        var adminEmail = "admin@meditrack.com";
        var adminPassword = "Admin123!";

        var existingAdmin = await userRepository.GetByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {

            var registerDto = new RegisterDto(
                Email: adminEmail,
                Password: adminPassword,
                FirstName: "Super",
                LastName: "Admin",
                PhoneNumber: "+1234567890",
                Type: UserType.Admin  // ← 5
            );

            await authService.RegisterAsync(registerDto);
            
            var createdAdmin = await userRepository.GetByEmailAsync(adminEmail);
        }
        else
        {
        }
    }
    catch (Exception ex)
    {
    }

}
    }
}