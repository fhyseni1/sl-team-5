using AutoMapper;
using MedicationService.API.Services;
using MedicationService.Application.Interfaces;
using MedicationService.Application.Mappings;
using MedicationService.Infrastructure.Data;
using MedicationService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MedicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<IMedicationRepository, MedicationRepository>();
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();

builder.Services.AddScoped<IMedicationService,Application.Services.MedicationService>();
builder.Services.AddScoped<IPrescriptionService,Application.Services.MedicationService>();


builder.Services.AddAutoMapper(typeof(MedicationProfile));
builder.Services.AddAutoMapper(typeof(PrescriptionProfile));


builder.Services.AddGrpc();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
