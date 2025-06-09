using Domain.Entities;
using Domain.Enums;
using Infrastructure.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Web.Components.Pages
{
    public partial class Dashboard : ComponentBase
    {
        private EstacionesEntity? EstacionEntity;
        private IEnumerable<InventarioEntity>? InventarioEntity;
        private EstacionViewModel? EstacionViewModel;
        private List<TanqueViewModel>? ListTanques;
        private List<ProductoViewModel>? ListProductos;

        private bool IsLoading = true;

        protected override async Task OnInitializedAsync()
        {

            int idEstacion = Configuration.GetValue<int>("Estacion:Id");
            EstacionEntity = await GetEstacionesByIdUseCase.ExecuteAsync(idEstacion);
            InventarioEntity = await GetInventarioUseCase.ExecuteAsync(s => s.Idestacion == EstacionEntity.Id);

            CreateEstacionViewModel(EstacionEntity);
            CreateTanquesViewModel(InventarioEntity);
            CreateProductosViewModel(ListTanques);
            IsLoading = false;
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
