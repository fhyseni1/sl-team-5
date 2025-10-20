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

// ========================================
// JWT CONFIGURATION
// ========================================
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// ========================================
// REPOSITORIES
// ========================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAllergyRepository, AllergyRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// ========================================
// SERVICES
// ========================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAllergyService, AllergyService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// ========================================
// HTTP CONTEXT ACCESSOR
// ========================================
builder.Services.AddHttpContextAccessor();

// ========================================
// AUTOMAPPER
// ========================================
builder.Services.AddAutoMapper(typeof(AllergyProfile), typeof(AppointmentProfile));

// ========================================
// AUTHENTICATION & JWT
// ========================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["access_token"];
                return Task.CompletedTask;
            }
        };
    });

// ========================================
// CORS - FIXED
// ========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllDev", policy =>
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

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