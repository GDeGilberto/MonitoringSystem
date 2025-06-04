using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class DescargasRepository : IRepository<DescargasEntity>, IRepositorySearch<ProcDescargaModel, DescargasEntity>
    {
        private readonly AppDbContext _dbContext;
        public DescargasRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(DescargasEntity entity)
        {
            try
            {
                ProcDescargaModel descarga = new()
                {
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
                var isRegistered = await _dbContext.ProcDescargas
                    .FirstOrDefaultAsync(d => d.IdEstacion == descarga.IdEstacion
                        && d.NoTanque == descarga.NoTanque
                        && d.FechaInicio.Date == descarga.FechaInicio.Date);

                if (isRegistered == null)
                {
                    await _dbContext.ProcDescargas.AddAsync(descarga);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Error de base de datos Mensaje: {dbEx.Message}");
            }
        }

        public async Task<IEnumerable<DescargasEntity>> GetAllAsync()
            => await _dbContext.ProcDescargas
                .Select(d => new DescargasEntity
                (
                    d.Id,
                    d.IdEstacion,
                    d.NoTanque,
                    d.VolumenInicio,
                    d.TemperaturaInicio,
                    d.FechaInicio,
                    d.VolumenDisponible,
                    d.TemperaturaFinal,
                    d.FechaFinal,
                    d.CantidadCargada
                )).ToListAsync();

        public async Task<IEnumerable<DescargasEntity>> GetAsync(Expression<Func<ProcDescargaModel, bool>> predicate)
        {
            var descargasModel = await _dbContext.ProcDescargas
                .Where(predicate)
                .ToListAsync();

            return descargasModel.Select(d => new DescargasEntity
                (
                    d.Id,
                    d.IdEstacion,
                    d.NoTanque,
                    d.VolumenInicio,
                    d.TemperaturaInicio,
                    d.FechaInicio,
                    d.VolumenDisponible,
                    d.TemperaturaFinal,
                    d.FechaFinal,
                    d.CantidadCargada
                ));
        }

        public async Task<DescargasEntity> GetByIdAsync(int id)
        {
            var descargaModel = await _dbContext.ProcDescargas.FindAsync(id);
            return new DescargasEntity
                (
                    descargaModel.Id,
                    descargaModel.IdEstacion,
                    descargaModel.NoTanque,
                    descargaModel.VolumenInicio,
                    descargaModel.TemperaturaInicio,
                    descargaModel.FechaInicio,
                    descargaModel.VolumenDisponible,
                    descargaModel.TemperaturaFinal,
                    descargaModel.FechaFinal,
                    descargaModel.CantidadCargada
                );
        }
    }
}
