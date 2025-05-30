using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DescargasRepository : IRepository<ProcDescargasEntity>
    {
        private readonly AppDbContext _dbContext;
        public DescargasRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(ProcDescargasEntity entity)
        {
            try
            {
                ProcDescargaModel descarga = new ProcDescargaModel
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

                await _dbContext.ProcDescargas.AddAsync(descarga);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Error de base de datos Mensaje: {dbEx.Message}");
            }
        }

        public Task<IEnumerable<ProcDescargasEntity>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ProcDescargasEntity> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
