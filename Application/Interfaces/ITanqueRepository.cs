using Domain.Entities;

namespace Application.Interfaces
{
    public interface ITanqueRepository
    {
        Task<TanqueEntity?> GetTanqueByEstacionAndNumeroAsync(int idEstacion, int noTanque);
        Task<IEnumerable<TanqueEntity>> GetAllAsync();
    }
}