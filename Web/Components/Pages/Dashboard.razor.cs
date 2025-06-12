using Application.UseCases;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Models;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Web.Components.Pages
{
    public partial class Dashboard : ComponentBase
    {
        [Inject] private GetEstacionesByIdUseCase GetEstacionesByIdUseCase { get; set; }
        [Inject] private GetLatestInventarioByStationUseCase<ProcInventarioModel> GetInventarioUseCase { get; set; }
        [Inject] private IConfiguration Configuration { get; set; }

        private EstacionesEntity? EstacionEntity;
        private IEnumerable<InventarioEntity>? InventarioEntity;
        private EstacionViewModel? EstacionViewModel;
        private List<TanqueViewModel>? ListTanques;
        private List<ProductoViewModel>? ListProductos;

        private DateTime dateUpdate { get; set; } = DateTime.Now;

        private bool IsLoadingEstacion = true;
        private bool IsLoadingProductos = true;
        private bool IsLoadingChart = true;
        private bool IsLoadingTanques = true;


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                int idEstacion = Configuration.GetValue<int>("Estacion:Id");
                EstacionEntity = await GetEstacionesByIdUseCase.ExecuteAsync(idEstacion);
                InventarioEntity = await GetInventarioUseCase.ExecuteAsync(s => s.Idestacion == EstacionEntity.Id);

                CreateEstacionViewModel(EstacionEntity);
                IsLoadingEstacion = false;
                CreateTanquesViewModel(InventarioEntity);
                IsLoadingTanques = false;
                IsLoadingChart = false;
                CreateProductosViewModel(ListTanques);
                IsLoadingProductos = false;

                dateUpdate = DateTime.Now;
                StateHasChanged();
            }
        }

        public async Task UpdateInventory()
        {
            try
            {
                IsLoadingProductos = true;
                IsLoadingTanques = true;

                int idEstacion = Configuration.GetValue<int>("Estacion:Id");
                EstacionEntity = await GetEstacionesByIdUseCase.ExecuteAsync(idEstacion);
                InventarioEntity = await GetInventarioUseCase.ExecuteAsync(s => s.Idestacion == EstacionEntity.Id);
                dateUpdate = DateTime.Now;

                CreateTanquesViewModel(InventarioEntity);
                IsLoadingTanques = false;
                IsLoadingChart = false;
                CreateProductosViewModel(ListTanques);
                IsLoadingProductos = false;       
            }
            finally
            {
                StateHasChanged();
            }
        }


        private void CreateEstacionViewModel(EstacionesEntity? estacionesEntity)
        {
            if (estacionesEntity != null)
            {
                EstacionViewModel = new EstacionViewModel
                {
                    Id = estacionesEntity.Id,
                    Nombre = estacionesEntity.Nombre
                };
            }
        }

        private void CreateTanquesViewModel(IEnumerable<InventarioEntity> inventarioEntity)
        {
            ListTanques = (from info in inventarioEntity
                           join t in EstacionEntity.Tanques on info.NoTanque equals int.Parse(t.NoTanque)
                           select new TanqueViewModel
                           {
                               NoTanque = info.NoTanque ?? 0,
                               Producto = Enum.TryParse<TipoProducto>(info.ClaveProducto, out var tipo)
                                ? tipo
                                : TipoProducto.Desconocido,
                               Volumen = (decimal?)info.VolumenDisponible,
                               Capacidad = t.Capacidad,
                               Temperatura = (decimal?)info.Temperatura
                           }).ToList();
        }

        private void CreateProductosViewModel(List<TanqueViewModel>? tanques)
        {
            ListProductos = tanques?
                .GroupBy(t => t.Producto)
                .Select(g => new ProductoViewModel
                {
                    Nombre = g.Key.ToString(),
                    Cantidad = g.Sum(t => t.Volumen ?? 0)
                }).ToList();
        }
    }
}
