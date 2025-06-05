using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using Domain.Entities;
using Hangfire;
using Infrastructure.Communication;
using Infrastructure.Data;
using Infrastructure.Dtos;
using Infrastructure.Jobs;
using Infrastructure.Mappers;
using Infrastructure.Models;
using Infrastructure.Presenters;
using Infrastructure.Repositories;
using Infrastructure.ViewModels;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddScoped<IRepository<DescargasEntity>, DescargasRepository>();
builder.Services.AddScoped<IRepository<EstacionesEntity>, EstacionesRepository>();
builder.Services.AddScoped<IRepository<InventarioEntity>, InventarioRepository>();
builder.Services.AddScoped<IRepository<DescargasEntity>, DescargasRepository>();
builder.Services.AddScoped<IRepositorySearch<ProcDescargaModel, DescargasEntity>, DescargasRepository>();
builder.Services.AddScoped<IRepositorySearch<ProcInventarioModel, InventarioEntity>, InventarioRepository>();
builder.Services.AddScoped<IMapper<DescargaRequestDTO, DescargasEntity>, DescargaMapper>();
builder.Services.AddScoped<IMapper<InventarioRequestDTO, InventarioEntity>, InventarioMapper>();
builder.Services.AddScoped<IPresenter<InventarioEntity, InventarioViewModel>, InventarioPresenter>();
builder.Services.AddScoped<DescargasService<DescargasEntity>>();
builder.Services.AddScoped<DescargasJobs>();
builder.Services.AddScoped<CreateDescargasUseCase<DescargaRequestDTO>>();
builder.Services.AddScoped<CreateInventarioUseCase<InventarioRequestDTO>>();
builder.Services.AddScoped<GetDescargasUseCase>();
builder.Services.AddScoped<GetDescargaByIdUseCase>();
builder.Services.AddScoped<GetDescargaSearchUseCase<ProcDescargaModel>>();
builder.Services.AddScoped<GetEstacionesUseCase>();
builder.Services.AddScoped<GetEstacionesByIdUseCase>();   
builder.Services.AddScoped<GetInventariosUseCase>();
builder.Services.AddScoped<GetInventarioByIdUseCase>();
builder.Services.AddScoped<GetLatestInventarioByStationUseCase<ProcInventarioModel>>();
builder.Services.AddScoped<ParceDeliveryReport>();
builder.Services.AddScoped<InventarioService<InventarioEntity, InventarioViewModel>>();
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

RecurringJob.AddOrUpdate<InventarioJob>(
    "job-volumenYTemperatura-de-tanques-3-minutos",
    job => job.Execute(),
    "*/3 * * * *"); // Every 3 minutes
RecurringJob.AddOrUpdate<DescargasJobs>(
    "job-Cargas-a-los-tanques",
    job => job.Execute(),
    Cron.Daily(6)); // 6:00 AM daily

app.MapControllers();

app.MapGet("/inventarios/{idEstacion}", async (
    GetLatestInventarioByStationUseCase<ProcInventarioModel> getInventariosUseCase,
    int idEstacion) =>
{
    var result = await getInventariosUseCase.ExecuteAsync(s => s.Idestacion == idEstacion);
    return Results.Ok(result);
}).WithName("GetInventariosByIdEstacion");

app.MapGet("/inventarios{id}", async (GetInventarioByIdUseCase getInventarioUseCase,
    int id) =>
{
    var result = await getInventarioUseCase.ExecuteAsync(id);
    return Results.Ok(result);
}).WithName("GetInventariosById");

app.MapPost("/inventario", async (InventarioRequestDTO inventarioRequest,
    CreateInventarioUseCase<InventarioRequestDTO> inventarioUseCase) =>
{
    await inventarioUseCase.ExecuteAsync(inventarioRequest);
    return Results.Created();
}).WithName("AddInventario");

app.MapGet("/descargas", async (GetDescargasUseCase getDescargasUseCase) =>
{
    var result = await getDescargasUseCase.ExecuteAsync();
    return Results.Ok(result);
}).WithName("GetDescargas");

app.MapGet("/descargas/{id}", async (GetDescargaByIdUseCase getDescargasUseCase, 
    int id) =>
{
    var result = await getDescargasUseCase.ExecuteAsync(id);
    return Results.Ok(result);
}).WithName("GetDescargaById");

app.MapGet("/descargas/{idEstacion}&{noTanque}", async (GetDescargaSearchUseCase<ProcDescargaModel> descargaSearchUseCase,
    int idEstacion, int noTanque) =>
{
    var result = await descargaSearchUseCase.ExecuteAsync(d => d.IdEstacion == idEstacion && d.NoTanque == noTanque);
    return result.Any() ? Results.Ok(result) : Results.NotFound();
}).WithName("GetDescargaByQuery");

app.MapPost("/descargas", async (DescargaRequestDTO descargaRequest, 
    CreateDescargasUseCase<DescargaRequestDTO> descargasUseCase) =>
{
    await descargasUseCase.ExecuteAsync(descargaRequest);
    return Results.Created();
}).WithName("AddDescarga");

app.MapGet("/estaciones", async (GetEstacionesUseCase useCase) =>
{
    var result = await useCase.ExecuteAsync();
    return Results.Ok(result);
}).WithName("GetEstaciones");

app.MapGet("/estaciones/{id}", async (GetEstacionesByIdUseCase useCase, 
    int id) =>
{
    var result = await useCase.ExecuteAsync(id);
    return result != null ? Results.Ok(result) : Results.NotFound();
}).WithName("GetEstacionById");


app.Run();
