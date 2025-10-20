// UserHealthService.API/Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System;
using UserHealthService.Application.Options;
using UserHealthService.Application.Interfaces;
using UserHealthService.Application.Mappings;
using UserHealthService.Application.Services;
using UserHealthService.Infrastructure.Data;
using UserHealthService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// DATABASE
// ========================================
builder.Services.AddDbContext<UserHealthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// ========================================
// JWT CONFIGURATION
// ========================================
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// ========================================
// REPOSITORIES
// ========================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Dependency Injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IAllergyRepository, AllergyRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// ========================================
// SERVICES
// ========================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAllergyService, AllergyService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IHealthMetricRepository, HealthMetricRepository>();
builder.Services.AddScoped<IHealthMetricService, HealthMetricService>();
builder.Services.AddScoped<ISymptomLogRepository, SymptomLogRepository>();
builder.Services.AddScoped<ISymptomLogService, SymptomLogService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(UserProfile), typeof(AllergyProfile), typeof(AppointmentProfile), typeof(HealthMetricProfile), typeof(SymptomLogProfile));

// ========================================
// API CONTROLLERS & SWAGGER
// ========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UserHealthService API",
        Version = "v1",
        Description = "Complete Health Management API with JWT Authentication"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ========================================
// CRITICAL MIDDLEWARE ORDER - FIXED
// ========================================
if (app.Environment.IsDevelopment())
{
    // ✅ SWAGGER FIRST
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "UserHealthService API v1");
        options.RoutePrefix = "swagger"; // Swagger at root: https://localhost:7108/
        // ✅ FIXED: DisplayRequestDuration is a METHOD
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAllDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
