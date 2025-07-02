using Application.Interfaces;
using Application.Services;
using Application.UseCases;
using Domain.Entities;
using Infrastructure.Communication;
using Infrastructure.Data;
using Infrastructure.Dtos;
using Infrastructure.Mappers;
using Infrastructure.Models;
using Infrastructure.Presenters;
using Infrastructure.Repositories;
using Infrastructure.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
                
            return services;
        }
        
        public static IServiceCollection AddSerialPortServices(this IServiceCollection services)
        {
            // Register SerialPortManager as singleton to avoid conflicts and support async operations
            services.AddSingleton<ISerialPortService, SerialPortManager>();
            
            return services;
        }
        
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IRepository<DescargasEntity>, DescargasRepository>();
            services.AddScoped<IRepository<EstacionesEntity>, EstacionesRepository>();
            services.AddScoped<IRepository<InventarioEntity>, InventarioRepository>();
            services.AddScoped<IRepository<TanqueEntity>, TanqueRepository>();
            services.AddScoped<ITanqueRepository, TanqueRepository>();
            services.AddScoped<IRepositorySearch<ProcDescargaModel, DescargasEntity>, DescargasRepository>();
            services.AddScoped<IRepositorySearch<ProcInventarioModel, InventarioEntity>, InventarioRepository>();
            
            return services;
        }
        
        public static IServiceCollection AddMappers(this IServiceCollection services)
        {
            services.AddScoped<IMapper<DescargaRequestDTO, DescargasEntity>, DescargaMapper>();
            services.AddScoped<IMapper<InventarioRequestDTO, InventarioEntity>, InventarioMapper>();
            services.AddScoped<IPresenter<InventarioEntity, InventarioViewModel>, InventarioPresenter>();
            
            return services;
        }
        
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<DescargasService<DescargasEntity>>();
            services.AddScoped<InventarioService<InventarioEntity, InventarioViewModel>>();
            
            // Add HttpClient for SOAP service
            services.AddHttpClient<IDagalSoapService, DagalSoapService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "MonitoringSystem/1.0");
            });
            
            return services;
        }
        
        public static IServiceCollection AddUseCases(this IServiceCollection services)
        {
            services.AddScoped<CreateDescargasUseCase<DescargaRequestDTO>>();
            services.AddScoped<CreateInventarioUseCase<InventarioRequestDTO>>();
            services.AddScoped<GetDescargasUseCase>();
            services.AddScoped<GetDescargaByIdUseCase>();
            services.AddScoped<GetDescargaSearchUseCase<ProcDescargaModel>>();
            services.AddScoped<GetEstacionesUseCase>();
            services.AddScoped<GetEstacionesByIdUseCase>();   
            services.AddScoped<GetInventariosUseCase>();
            services.AddScoped<GetInventarioByIdUseCase>();
            services.AddScoped<GetLatestInventarioByStationUseCase<ProcInventarioModel>>();
            services.AddScoped<GetTanqueByEstacionAndNumeroUseCase>();
            
            return services;
        }
    }
}