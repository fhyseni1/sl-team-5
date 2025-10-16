using Microsoft.EntityFrameworkCore;
using MedicationService.Infrastructure.Data;
using MedicationService.API.Services;
using MedicationService.Application.Interfaces;
using MedicationService.Infrastructure.Repositories;
using MedicationService.Application.Mappings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MedicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<IMedicationRepository, MedicationRepository>();
builder.Services.AddScoped<IMedicationService, Application.Services.MedicationService>();

builder.Services.AddAutoMapper(typeof(MedicationProfile));

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
