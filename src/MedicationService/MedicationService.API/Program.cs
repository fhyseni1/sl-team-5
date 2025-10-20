using AutoMapper;
using MedicationService.API.Services;
using MedicationService.Application.Interfaces;
using MedicationService.Application.Mappings;
using MedicationService.Infrastructure.Data;
using MedicationService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using MedicationService.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<MedicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Repositories
builder.Services.AddScoped<IMedicationRepository, MedicationRepository>();
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IMedicationDoseRepository, MedicationDoseRepository>();
builder.Services.AddScoped<IDrugInteractionRepository, DrugInteractionRepository>();
builder.Services.AddScoped<IMedicationScheduleRepository, MedicationScheduleRepository>();
builder.Services.AddScoped<IMedicationReminderRepository, MedicationReminderRepository>();

// Services
builder.Services.AddScoped<IMedicationService, MedicationService.Application.Services.MedicationService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IMedicationDoseService, MedicationDoseService>();
builder.Services.AddScoped<IDrugInteractionService, DrugInteractionService>();
builder.Services.AddScoped<IMedicationScheduleService, MedicationScheduleService>();
builder.Services.AddScoped<IMedicationReminderService, MedicationReminderService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MedicationProfile).Assembly);
builder.Services.AddAutoMapper(typeof(MedicationScheduleProfile).Assembly);
builder.Services.AddAutoMapper(typeof(PrescriptionProfile).Assembly);
builder.Services.AddAutoMapper(typeof(MedicationDoseProfile).Assembly);
builder.Services.AddAutoMapper(typeof(DrugInteractionProfile).Assembly);
builder.Services.AddAutoMapper(typeof(MedicationReminderProfile).Assembly);


// gRPC & Controllers
builder.Services.AddGrpc();
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGrpcService<MedicationGrpcService>();
app.MapGet("/", () => "MedicationService gRPC Server is running.");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
