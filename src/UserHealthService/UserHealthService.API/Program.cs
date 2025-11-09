using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Net.Http.Headers;
using System.Text;
using UserHealthService.Application.DTOs.Clinics;
using UserHealthService.Application.DTOs.Doctors;
using UserHealthService.Application.DTOs.Relationships;
using UserHealthService.Application.Interfaces;
using UserHealthService.Application.Mappings;
using UserHealthService.Application.Options;
using UserHealthService.Application.Services;
using UserHealthService.Domain.Entities;
using UserHealthService.Infrastructure.Data;
using UserHealthService.Infrastructure.Repositories;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Dashboard;

var builder = WebApplication.CreateBuilder(args);
// Add Hangfire services
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Hangfire server
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1;
});


builder.Services.AddDbContext<UserHealthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAllergyRepository, AllergyRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IHealthMetricRepository, HealthMetricRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAllergyService, AllergyService>();
builder.Services.AddScoped<IHealthMetricRepository, HealthMetricRepository>();
builder.Services.AddScoped<IHealthMetricService, HealthMetricService>();
builder.Services.AddScoped<ISymptomLogRepository, SymptomLogRepository>();
builder.Services.AddScoped<ISymptomLogService, SymptomLogService>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IUserRelationshipRepository, UserRelationshipRepository>();
builder.Services.AddScoped<IUserRelationshipService, UserRelationshipService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IClinicRepository, ClinicRepository>();
builder.Services.AddScoped<IPDFReportService, PDFReportService>();
builder.Services.AddScoped<IAppointmentReportRepository, AppointmentReportRepository>();
builder.Services.AddScoped<IAppointmentReportService, AppointmentReportService>();
builder.Services.AddScoped<IMedicationReminderJob, MedicationReminderJob>();
builder.Services.AddScoped<MedicationReminderJob>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddHttpClient("MedicationService", (sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["MedicationService:Url"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }

    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});


builder.Services.AddAutoMapper(
    typeof(UserHealthService.Application.Mappings.UserProfile), 
    typeof(AllergyProfile),
    typeof(AppointmentProfile),
    typeof(AppointmentReportProfile), 
    typeof(HealthMetricProfile), 
    typeof(SymptomLogProfile),
    typeof(NotificationProfile),
    typeof(UserRelationshipProfile)
);

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Doctor", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("Assistant", policy => policy.RequireRole("Assistant"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
            .WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:3002",
                "https://localhost:3002")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true)  // Be careful with this in production
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));
});

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
// Configure Hangfire dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "UserHealthService Jobs",
    DarkModeEnabled = true
});

// Schedule recurring job
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    
    // 
    //  CHANGE TO CONCRETE CLASS
    recurringJobManager.AddOrUpdate<MedicationReminderJob>(
        "medication-reminders",
        job => job.ProcessReminders(),
        Cron.Minutely);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "UserHealthService API v1");
        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
        app.UseDeveloperExceptionPage();
    });
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
await UserHealthService.Infrastructure.Data.SeedAdmin.EnsureSuperAdminAsync(app.Services);
app.Run();
// Hangfire Authorization Filter - MBET NË FUND
public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var environment = httpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        
        if (environment.IsDevelopment())
        {
            return true;
        }

        return httpContext.User.Identity?.IsAuthenticated == true && 
               httpContext.User.IsInRole("Admin");
    }
}