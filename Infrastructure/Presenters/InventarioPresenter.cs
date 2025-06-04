using Application.Interfaces;
using Domain.Entities;
using Infrastructure.ViewModels;

namespace Infrastructure.Presenters
{
    public class InventarioPresenter : IPresenter<InventarioEntity, InventarioViewModel>
    {
        public IEnumerable<InventarioViewModel> Present(IEnumerable<InventarioEntity> data)
        {
            return data.Select(i => new InventarioViewModel
            {
                NombreEstacion = i.IdEstacion.ToString(),
                NoTank = (int)i.NoTanque,
                NombreProducto = i.ClaveProducto,
                VolumenDisponible = (float)i.VolumenDisponible,
                Temperatura = (float)i.Temperatura,
                Fecha = (DateTime)i.Fecha
            });
        }
    }
}
