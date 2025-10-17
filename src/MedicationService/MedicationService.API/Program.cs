using AutoMapper;
using MedicationService.API.Services;
using MedicationService.Application.Interfaces;
using MedicationService.Application.Mappings;
using MedicationService.Infrastructure.Data;
using MedicationService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using MedicationService.Application.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddDbContext<MedicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Repositories
builder.Services.AddScoped<IMedicationRepository, MedicationRepository>();
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IMedicationDoseRepository, MedicationDoseRepository>();

// Services
builder.Services.AddScoped<IMedicationDoseService, MedicationDoseService>();


// AutoMapper
builder.Services.AddAutoMapper(typeof(MedicationProfile)); 
builder.Services.AddAutoMapper(typeof(MedicationDoseProfile));

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
app.UseCors("AllowNextJs");
app.UseAuthorization();

app.MapControllers();

app.Run();
