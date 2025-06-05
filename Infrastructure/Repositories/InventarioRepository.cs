using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class InventarioRepository : IRepository<InventarioEntity>, IRepositorySearch<ProcInventarioModel, InventarioEntity>
    {
        private readonly AppDbContext _dbContext;

        public InventarioRepository(AppDbContext dbContext)
            => _dbContext = dbContext;

        public async Task AddAsync(InventarioEntity entity)
        {
            var tanque = await _dbContext.CatTanques
                .FirstOrDefaultAsync(t =>
                    t.IdEstacion == entity.IdEstacion &&
                    t.NoTanque == /*(entity.NoTanque < 10 ? $"0{entity.NoTanque}" :*/ entity.NoTanque.ToString());

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
                NoTanque =  entity.NoTanque.ToString(),
                ClaveProducto = tanque.Producto,
                VolumenDisponible = entity.VolumenDisponible,
                Temperatura = entity.Temperatura,
                Fecha = entity.Fecha
            };
            await _dbContext.ProcInventarios.AddAsync(inventoryModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<InventarioEntity>> GetAllAsync()
            => await _dbContext.ProcInventarios.Select(i => new InventarioEntity
            (
                i.Idestacion,
                string.IsNullOrEmpty(i.NoTanque) ? null : int.Parse(i.NoTanque),
                i.ClaveProducto,
                i.VolumenDisponible,
                i.Temperatura,
                i.Fecha
            )).ToListAsync();

        public async Task<InventarioEntity> GetByIdAsync(int id)
        {
            var inventario = await _dbContext.ProcInventarios.FindAsync(id);
            return new InventarioEntity
            (
                inventario.Idestacion,
                int.Parse(inventario.NoTanque),
                inventario.ClaveProducto,
                inventario.VolumenDisponible,
                inventario.Temperatura,
                inventario.Fecha
            );
        }

        public async Task<IEnumerable<InventarioEntity>> GetAsync(Expression<Func<ProcInventarioModel, bool>> predicate)
        {
            var query = from i in _dbContext.ProcInventarios.Where(predicate)
                        join maxFechas in
                            (
                                from x in _dbContext.ProcInventarios.Where(predicate)
                                group x by x.NoTanque into g
                                select new { NoTanque = g.Key, Fecha = g.Max(x => x.Fecha) }
                            )
                            on new { i.NoTanque, i.Fecha } equals new { maxFechas.NoTanque, Fecha = maxFechas.Fecha }
                        select new InventarioEntity(
                            i.Idestacion,
                            string.IsNullOrEmpty(i.NoTanque) ? null : int.Parse(i.NoTanque),
                            i.ClaveProducto,
                            i.VolumenDisponible,
                            i.Temperatura,
                            i.Fecha
                        );

            return await query.ToListAsync();
        }
    }
}
