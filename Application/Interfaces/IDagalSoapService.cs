using Domain.Entities;

namespace Application.Interfaces
{
    public interface IDagalSoapService
    {
        Task<bool> RegistrarEstatusInventarioAsync(InventarioEntity inventario);
    }
}