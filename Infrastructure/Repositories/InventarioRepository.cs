using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class InventarioRepository : IRepository<ProcInventarioEntity>
    {
        private readonly AppDbContext _dbContext;

        public InventarioRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(ProcInventarioEntity entity)
        {
            // Buscar el tanque correspondiente
            var tanque = await _dbContext.CatTanques
                .FirstOrDefaultAsync(t =>
                    t.IdEstacion == entity.IdEstacion &&
                    t.NoTanque == (entity.NoTanque < 10 ? $"0{entity.NoTanque}" : entity.NoTanque.ToString()));

            if (tanque == null)
            {
                throw new InvalidOperationException($"No se encontró el tanque {entity.NoTanque} en la estación {entity.IdEstacion}");
            }

            if (string.IsNullOrEmpty(tanque.Producto))
            {
                throw new InvalidOperationException("El tanque no tiene un producto asignado");
            }

            var inventoryModel = new ProcInventarioModel
            {
                Idestacion = entity.IdEstacion,
                NoTanque = entity.NoTanque < 10
                        ? $"0{entity.NoTanque}"
                        : entity.NoTanque.ToString(),
                ClaveProducto = tanque.Producto,
                VolumenDisponible = entity.VolumenDisponible,
                Temperatura = entity.Temperatura,
                Fecha = entity.Fecha
            };
            await _dbContext.ProcInventarios.AddAsync(inventoryModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProcInventarioEntity>> GetAllAsync()
        {
            return await _dbContext.ProcInventarios.Select(i => new ProcInventarioEntity
            {
                IdReg = i.IdReg,
                IdEstacion = i.Idestacion,
                NoTanque = string.IsNullOrEmpty(i.NoTanque) ? null : int.Parse(i.NoTanque),
                ClaveProducto = i.ClaveProducto,
                VolumenDisponible = i.VolumenDisponible,
                Temperatura = i.Temperatura,
                Fecha = i.Fecha
            }).ToListAsync();
        }

        public async Task<ProcInventarioEntity> GetByIdAsync(int id)
        {
            var inventario = await _dbContext.ProcInventarios.FindAsync(id);
            return new ProcInventarioEntity
            {
                IdReg = inventario.IdReg,
                IdEstacion = inventario.Idestacion,
                NoTanque = int.Parse(inventario.NoTanque),
                ClaveProducto = inventario.ClaveProducto,
                VolumenDisponible = inventario.VolumenDisponible,
                Temperatura = inventario.Temperatura,
                Fecha = inventario.Fecha
            };
        }
    }
}
