using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Models;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.Demo
{
    public class DemoInventarioRepository : IRepository<InventarioEntity>, IRepositorySearch<ProcInventarioModel, InventarioEntity>
    {
        private static readonly List<ProcInventarioModel> _inventarios = new()
        {
            new ProcInventarioModel
            {
                IdReg = 1,
                Idestacion = 11162,
                NoTanque = "1",
                ClaveProducto = "34006", // Magna
                VolumenDisponible = 8500.75,
                Temperatura = 26.2,
                Fecha = DateTime.Now.AddMinutes(-30)
            },
            new ProcInventarioModel
            {
                IdReg = 2,
                Idestacion = 11162,
                NoTanque = "2",
                ClaveProducto = "34007", // Premium
                VolumenDisponible = 7250.50,
                Temperatura = 25.1,
                Fecha = DateTime.Now.AddMinutes(-30)
            },
            new ProcInventarioModel
            {
                IdReg = 3,
                Idestacion = 11162,
                NoTanque = "3",
                ClaveProducto = "34008", // Diesel
                VolumenDisponible = 12750.25,
                Temperatura = 26.5,
                Fecha = DateTime.Now.AddMinutes(-30)
            },
            new ProcInventarioModel
            {
                IdReg = 4,
                Idestacion = 11162,
                NoTanque = "4",
                ClaveProducto = "34006", // Magna
                VolumenDisponible = 9850.00,
                Temperatura = 25.8,
                Fecha = DateTime.Now.AddMinutes(-30)
            },
            new ProcInventarioModel
            {
                IdReg = 5,
                Idestacion = 11162,
                NoTanque = "5",
                ClaveProducto = "34006", // Magna
                VolumenDisponible = 4850.00,
                Temperatura = 25.8,
                Fecha = DateTime.Now.AddMinutes(-30)
            }
        };

        public Task AddAsync(InventarioEntity entity)
        {
            var newInventario = new ProcInventarioModel
            {
                IdReg = _inventarios.Count + 1,
                Idestacion = entity.IdEstacion,
                NoTanque = entity.NoTanque.ToString(),
                ClaveProducto = entity.ClaveProducto,
                VolumenDisponible = entity.VolumenDisponible,
                Temperatura = entity.Temperatura,
                Fecha = entity.Fecha
            };
            
            _inventarios.Add(newInventario);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<InventarioEntity>> GetAllAsync()
        {
            var entities = _inventarios.Select(MapToEntity);
            return Task.FromResult(entities);
        }

        public Task<InventarioEntity> GetByIdAsync(int id)
        {
            var inventario = _inventarios.FirstOrDefault(i => i.IdReg == id);
            if (inventario == null)
                throw new KeyNotFoundException($"Inventario with ID {id} not found");

            return Task.FromResult(MapToEntity(inventario));
        }

        public Task UpdateAsync(InventarioEntity entity)
        {
            var existing = _inventarios.FirstOrDefault(i => i.IdReg == entity.Id);
            if (existing != null)
            {
                existing.VolumenDisponible = entity.VolumenDisponible;
                existing.Temperatura = entity.Temperatura;
                existing.Fecha = entity.Fecha;
            }
            
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var existing = _inventarios.FirstOrDefault(i => i.IdReg == id);
            if (existing != null)
            {
                _inventarios.Remove(existing);
            }
            
            return Task.CompletedTask;
        }

        // Implementation of IRepositorySearch interface
        public Task<IEnumerable<InventarioEntity>> GetAsync(Expression<Func<ProcInventarioModel, bool>> predicate)
        {
            var compiledPredicate = predicate.Compile();
            var filteredInventarios = _inventarios.Where(compiledPredicate).Select(MapToEntity);
            return Task.FromResult(filteredInventarios);
        }

        // Método específico para obtener el último inventario por estación
        public Task<IEnumerable<InventarioEntity>> GetLatestByStationAsync(int idEstacion)
        {
            var latestInventarios = _inventarios
                .Where(i => i.Idestacion == idEstacion)
                .GroupBy(i => i.NoTanque)
                .Select(g => g.OrderByDescending(i => i.Fecha).First())
                .Select(MapToEntity);
            
            return Task.FromResult(latestInventarios);
        }

        private static InventarioEntity MapToEntity(ProcInventarioModel model)
        {
            return new InventarioEntity(
                model.Idestacion,
                int.TryParse(model.NoTanque, out var tanque) ? tanque : 0,
                model.ClaveProducto,
                model.VolumenDisponible,
                model.Temperatura,
                model.Fecha
            );
        }
    }
}