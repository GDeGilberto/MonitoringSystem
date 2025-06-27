using Application.UseCases;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Jobs;
using Infrastructure.Models;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Components;
using Web.Services;

namespace Web.Components.Pages
{
    public partial class Dashboard : ComponentBase, IDisposable
    {
        [Inject] private InventarioJob? _inventarioJob { get; set; }
        [Inject] private GetEstacionesByIdUseCase GetEstacionesByIdUseCase { get; set; }
        [Inject] private GetLatestInventarioByStationUseCase<ProcInventarioModel> GetInventarioUseCase { get; set; }
        [Inject] private IConfiguration Configuration { get; set; }
        [Inject] private IInventoryUpdateService InventoryUpdateService { get; set; } = default!;

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

        protected override async Task OnInitializedAsync()
        {
            // Subscribe to automatic inventory updates
            InventoryUpdateService.OnInventoryUpdated += OnAutomaticInventoryUpdate;
            
            // Start the automatic update service
            await InventoryUpdateService.StartAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadInitialData();
            }
        }

        private async Task LoadInitialData()
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

        private async Task OnAutomaticInventoryUpdate()
        {
            try
            {
                // This method is called every 3 minutes by the service from a background thread
                // We need to use InvokeAsync to marshal to the UI thread
                await InvokeAsync(async () =>
                {
                    try
                    {
                        await UpdateInventory();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating inventory in UI thread: {ex.Message}");
                        // Log but don't rethrow to avoid breaking the periodic timer
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during automatic inventory update: {ex.Message}");
            }
        }
        
        public async Task ClickUpdateInventario()
        {
            IsLoadingChart = true;
            IsLoadingProductos = true;
            IsLoadingTanques = true;

            try
            {
                await _inventarioJob?.Execute()!;

                await UpdateInventory();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating inventory: {ex.Message}");
            }
            finally
            {
                IsLoadingChart = false;
                IsLoadingProductos = false;
                IsLoadingTanques = false;
            }
        }

        public async Task ToggleAutoUpdate()
        {
            try
            {
                if (InventoryUpdateService.IsRunning)
                {
                    await InventoryUpdateService.StopAsync();
                }
                else
                {
                    await InventoryUpdateService.StartAsync();
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling auto-update service: {ex.Message}");
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

        public void Dispose()
        {
            // Unsubscribe from events and stop the service
            if (InventoryUpdateService != null)
            {
                InventoryUpdateService.OnInventoryUpdated -= OnAutomaticInventoryUpdate;
            }
        }
    }
}
