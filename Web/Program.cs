using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Models;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Configuration EF
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRepository<EstacionesEntity>, EstacionesRepository>();
builder.Services.AddScoped<IRepository<InventarioEntity>, InventarioRepository>();
builder.Services.AddScoped<GetLatestInventarioByStationUseCase<ProcInventarioModel>>();
builder.Services.AddScoped<GetEstacionesByIdUseCase>();

builder.Services.AddScoped<IRepositorySearch<ProcInventarioModel, InventarioEntity>, InventarioRepository>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
