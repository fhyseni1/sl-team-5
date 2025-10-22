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

    Console.WriteLine("🚀 SEED STARTING..."); // ← LOUD CONSOLE LOG

    try
    {
        var adminEmail = "admin@meditrack.com";
        var adminPassword = "Admin123!";

        var existingAdmin = await userRepository.GetByEmailAsync(adminEmail);
        Console.WriteLine($"🔍 Existing admin: {existingAdmin != null}"); // ← DEBUG

        if (existingAdmin == null)
        {
            Console.WriteLine("🌟 CREATING Super Admin..."); // ← LOUD

            var registerDto = new RegisterDto(
                Email: adminEmail,
                Password: adminPassword,
                FirstName: "Super",
                LastName: "Admin",
                PhoneNumber: "+1234567890",
                Type: UserType.Admin  // ← 5
            );

            await authService.RegisterAsync(registerDto);
            
            // ✅ VERIFY IT WORKED
            var createdAdmin = await userRepository.GetByEmailAsync(adminEmail);
            Console.WriteLine($"✅ CREATED! Type: {createdAdmin.Type} ({(int)createdAdmin.Type})");
        }
        else
        {
            Console.WriteLine($"✅ EXISTS! Type: {existingAdmin.Type} ({(int)existingAdmin.Type})");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ SEED ERROR: {ex.Message}");
        Console.WriteLine($"❌ SEED STACK: {ex.StackTrace}");
    }

    Console.WriteLine("🏁 SEED FINISHED!");
}
    }
}