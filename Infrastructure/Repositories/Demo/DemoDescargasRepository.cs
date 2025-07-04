using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Models;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.Demo
{
    public class DemoDescargasRepository : IRepository<DescargasEntity>, IRepositorySearch<ProcDescargaModel, DescargasEntity>
    {
        private static readonly List<ProcDescargaModel> _descargas = new()
        {
            new ProcDescargaModel
            {
                Id = 1,
                IdEstacion = 11162,
                NoTanque = 1,
                VolumenInicio = 15000.50,
                TemperaturaInicio = 25.5,
                FechaInicio = DateTime.Now.AddDays(-5),
                VolumenDisponible = 8500.75,
                TemperaturaFinal = 26.2,
                FechaFinal = DateTime.Now.AddDays(-5).AddHours(2),
                CantidadCargada = 6499.75
            },
            new ProcDescargaModel
            {
                Id = 2,
                IdEstacion = 11162,
                NoTanque = 2,
                VolumenInicio = 12000.25,
                TemperaturaInicio = 24.8,
                FechaInicio = DateTime.Now.AddDays(-3),
                VolumenDisponible = 7250.50,
                TemperaturaFinal = 25.1,
                FechaFinal = DateTime.Now.AddDays(-3).AddHours(1.5),
                CantidadCargada = 4749.75
            },
            new ProcDescargaModel
            {
                Id = 3,
                IdEstacion = 11162,
                NoTanque = 3,
                VolumenInicio = 18500.00,
                TemperaturaInicio = 26.0,
                FechaInicio = DateTime.Now.AddDays(-1),
                VolumenDisponible = 12750.25,
                TemperaturaFinal = 26.5,
                FechaFinal = DateTime.Now.AddDays(-1).AddHours(3),
                CantidadCargada = 5749.75
            }
        };

        public Task AddAsync(DescargasEntity entity)
        {
            var newDescarga = new ProcDescargaModel
            {
                Id = _descargas.Count + 1,
                IdEstacion = entity.IdEstacion,
                NoTanque = entity.NoTanque,
                VolumenInicio = entity.VolumenInicial,
                TemperaturaInicio = entity.TemperaturaInicial,
                FechaInicio = entity.FechaInicial,
                VolumenDisponible = entity.VolumenDisponible,
                TemperaturaFinal = entity.TemperaturaFinal,
                FechaFinal = entity.FechaFinal,
                CantidadCargada = entity.CantidadCargada
            };
            
            _descargas.Add(newDescarga);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<DescargasEntity>> GetAllAsync()
        {
            var entities = _descargas.Select(MapToEntity);
            return Task.FromResult(entities);
        }

        public Task<DescargasEntity> GetByIdAsync(int id)
        {
            var descarga = _descargas.FirstOrDefault(d => d.Id == id);
            if (descarga == null)
                throw new KeyNotFoundException($"Descarga with ID {id} not found");

            return Task.FromResult(MapToEntity(descarga));
        }

        public Task UpdateAsync(DescargasEntity entity)
        {
            var existing = _descargas.FirstOrDefault(d => d.Id == entity.Id);
            if (existing != null)
            {
                existing.VolumenInicio = entity.VolumenInicial;
                existing.TemperaturaInicio = entity.TemperaturaInicial;
                existing.FechaInicio = entity.FechaInicial;
                existing.VolumenDisponible = entity.VolumenDisponible;
                existing.TemperaturaFinal = entity.TemperaturaFinal;
                existing.FechaFinal = entity.FechaFinal;
                existing.CantidadCargada = entity.CantidadCargada;
            }
            
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var existing = _descargas.FirstOrDefault(d => d.Id == id);
            if (existing != null)
            {
                _descargas.Remove(existing);
            }
            
            return Task.CompletedTask;
        }

        // Implementation of IRepositorySearch interface
        public Task<IEnumerable<DescargasEntity>> GetAsync(Expression<Func<ProcDescargaModel, bool>> predicate)
        {
            var compiledPredicate = predicate.Compile();
            var filteredDescargas = _descargas.Where(compiledPredicate).Select(MapToEntity);
            return Task.FromResult(filteredDescargas);
        }

        private static DescargasEntity MapToEntity(ProcDescargaModel model)
        {
            return new DescargasEntity(
                model.Id,
                model.IdEstacion,
                model.NoTanque,
                model.VolumenInicio,
                model.TemperaturaInicio,
                model.FechaInicio,
                model.VolumenDisponible,
                model.TemperaturaFinal,
                model.FechaFinal,
                model.CantidadCargada
            );
        }
    }
}