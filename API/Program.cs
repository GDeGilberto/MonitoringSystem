using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using Domain.Entities;
using Hangfire;
using Infrastructure.Communication;
using Infrastructure.Data;
using Infrastructure.Jobs;
using Infrastructure.Presenters;
using Infrastructure.Repositories;
using Infrastructure.ViewModels;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration EF
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuration Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));
builder.Services.AddHangfireServer();

// Configuración del servicio serial
builder.Services.AddSingleton<ISerialPortService>(provider =>
    new SerialPortService());
builder.Services.AddSingleton<ISerialPortService, SerialPortService>();
builder.Services.AddSingleton<ISerialPortService, SerialPortManager>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<IRepository<ProcInventarioEntity>, InventarioRepository>();
builder.Services.AddScoped<IRepository<ProcDescargasEntity>, DescargasRepository>();
builder.Services.AddScoped<IPresenter<ProcInventarioEntity, InventarioViewModel>, InventarioPresenter>();
builder.Services.AddScoped<DescargasService<ProcDescargasEntity>>();
builder.Services.AddScoped<DescargasJobs>();
builder.Services.AddScoped<ParceDeliveryReport>();
builder.Services.AddScoped<InventarioService<ProcInventarioEntity, InventarioViewModel>>();
builder.Services.AddScoped<InventarioJob>();
builder.Services.AddScoped<ParseTankInventoryReport>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseHangfireDashboard();

//RecurringJob.AddOrUpdate<InventarioJob>(
//    "job-inventario-cada-minuto",
//    job => job.Execute(),
//    Cron.Minutely); // Every minute
RecurringJob.AddOrUpdate<InventarioJob>(
    "job-volumenYTemperatura-de-tanques-3-minutos",
    job => job.Execute(),
    "*/3 * * * *"); // Every 3 minutes
RecurringJob.AddOrUpdate<DescargasJobs>(
    "job-Cargas-a-los-tanques",
    job => job.Execute(),
    "*/2 * * * *");
//Cron.Daily(6)); // 6:00 AM daily


app.MapControllers();

app.Run();
