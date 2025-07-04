using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories.Demo
{
    public class DemoTanqueRepository : IRepository<TanqueEntity>, ITanqueRepository
    {
        private static readonly List<TanqueEntity> _tanques = new()
        {
            new TanqueEntity(11162, "1", "34006", 100, 15000, 14250),
            new TanqueEntity(11162, "2", "34007", 100, 12000, 11400),
            new TanqueEntity(11162, "3", "34008", 100, 18000, 17100),
            new TanqueEntity(11162, "4", "34006", 100, 15000, 14250),
            new TanqueEntity(11162, "5", "34007", 100, 12000, 11400)
        };

        public Task AddAsync(TanqueEntity entity)
        {
            var newTanque = new TanqueEntity(
                entity.IdEstacion,
                entity.NoTanque,
                entity.Producto,
                entity.Fondeja,
                entity.Capacidad,
                entity.Capacidad95
            );
            
            _tanques.Add(newTanque);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<TanqueEntity>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<TanqueEntity>>(_tanques);
        }

        public Task<TanqueEntity> GetByIdAsync(int id)
        {
            // For demo purposes, we'll use the tank number as ID
            var tanque = _tanques.FirstOrDefault(t => int.Parse(t.NoTanque) == id);
            if (tanque == null)
                throw new KeyNotFoundException($"Tanque with ID {id} not found");

            return Task.FromResult(tanque);
        }

        public Task UpdateAsync(TanqueEntity entity)
        {
            // For demo purposes, find by NoTanque and replace
            var existingIndex = _tanques.FindIndex(t => t.NoTanque == entity.NoTanque && t.IdEstacion == entity.IdEstacion);
            if (existingIndex >= 0)
            {
                _tanques[existingIndex] = entity;
            }
            
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var existing = _tanques.FirstOrDefault(t => int.Parse(t.NoTanque) == id);
            if (existing != null)
            {
                _tanques.Remove(existing);
            }
            
            return Task.CompletedTask;
        }

        // ITanqueRepository implementation
        public Task<TanqueEntity?> GetTanqueByEstacionAndNumeroAsync(int idEstacion, int noTanque)
        {
            var tanque = _tanques.FirstOrDefault(t => t.IdEstacion == idEstacion && t.NoTanque == noTanque.ToString());
            return Task.FromResult(tanque);
        }

        // Additional method for backward compatibility
        public Task<TanqueEntity> GetByEstacionAndNumeroAsync(int idEstacion, string numeroTanque)
        {
            var tanque = _tanques.FirstOrDefault(t => t.IdEstacion == idEstacion && t.NoTanque == numeroTanque);
            if (tanque == null)
                throw new KeyNotFoundException($"Tanque {numeroTanque} not found for station {idEstacion}");

            return Task.FromResult(tanque);
        }

        public Task<IEnumerable<TanqueEntity>> GetByEstacionAsync(int idEstacion)
        {
            var tanques = _tanques.Where(t => t.IdEstacion == idEstacion);
            return Task.FromResult(tanques);
        }
    }
}