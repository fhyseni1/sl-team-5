using Microsoft.EntityFrameworkCore;
using UserHealthService.Infrastructure.Data;
using UserHealthService.Application.Interfaces;
using UserHealthService.Infrastructure.Repositories;
using UserHealthService.Application.Services;
using UserHealthService.Application.Mappings;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<UserHealthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Dependency Injection
builder.Services.AddScoped<IAllergyRepository, AllergyRepository>();
builder.Services.AddScoped<IAllergyService, AllergyService>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IHealthMetricRepository, HealthMetricRepository>();
builder.Services.AddScoped<IHealthMetricService, HealthMetricService>();
builder.Services.AddScoped<ISymptomLogRepository, SymptomLogRepository>();
builder.Services.AddScoped<ISymptomLogService, SymptomLogService>();
// AutoMapper
builder.Services.AddAutoMapper(typeof(AllergyProfile), typeof(AppointmentProfile), typeof(SymptomLogProfile));
builder.Services.AddAutoMapper(typeof(HealthMetricProfile));

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
