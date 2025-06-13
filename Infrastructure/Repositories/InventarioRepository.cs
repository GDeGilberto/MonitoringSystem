using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class InventarioRepository : IRepository<InventarioEntity>, IRepositorySearch<ProcInventarioModel, InventarioEntity>
    {
        private readonly AppDbContext _dbContext;

        public InventarioRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddAsync(InventarioEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "La entidad de inventario no puede ser nula");
            }

            try
            {
                var tanque = await _dbContext.CatTanques
                    .FirstOrDefaultAsync(t =>
                        t.IdEstacion == entity.IdEstacion &&
                        t.NoTanque == (entity.NoTanque < 10 ? $"0{entity.NoTanque}" : entity.NoTanque.ToString()));

                if (tanque == null)
                {
                    throw new EntityNotFoundException($"No se encontró el tanque {entity.NoTanque} en la estación {entity.IdEstacion}");
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
                
                // Verificar si ya existe un registro idéntico
                var isRegistered = await _dbContext.ProcInventarios
                    .AnyAsync(i => i.Idestacion == inventoryModel.Idestacion
                        && i.NoTanque == inventoryModel.NoTanque
                        && i.Fecha == inventoryModel.Fecha);

                if (isRegistered)
                {
                    throw new DuplicateEntityException(
                        $"Ya existe un registro de inventario para la estación {entity.IdEstacion}, " +
                        $"tanque {entity.NoTanque} en la fecha {entity.Fecha}");
                }

                await _dbContext.ProcInventarios.AddAsync(inventoryModel);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new DatabaseConnectionException(
                    $"Error al guardar el inventario en la base de datos: {dbEx.Message}", dbEx);
            }
            catch (SqlException sqlEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de SQL al agregar el inventario: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex) when (!(ex is RepositoryException) && !(ex is ArgumentNullException) && !(ex is InvalidOperationException))
            {
                throw new RepositoryException(
                    $"Error inesperado al agregar el inventario: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InventarioEntity>> GetAllAsync()
        {
            try
            {
                var result = await _dbContext.ProcInventarios
                    .Select(i => new InventarioEntity
                    (
                        i.Idestacion,
                        string.IsNullOrEmpty(i.NoTanque) ? null : int.Parse(i.NoTanque),
                        i.ClaveProducto,
                        i.VolumenDisponible,
                        i.Temperatura,
                        i.Fecha
                    ))
                    .ToListAsync();

                return result;
            }
            catch (SqlException sqlEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de conexión SQL al obtener todos los inventarios: {sqlEx.Message}", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de base de datos al obtener todos los inventarios: {dbEx.Message}", dbEx);
            }
            catch (Exception ex) when (!(ex is RepositoryException))
            {
                throw new RepositoryException(
                    $"Error inesperado al obtener todos los inventarios: {ex.Message}", ex);
            }
        }

        public async Task<InventarioEntity> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser un valor positivo", nameof(id));
            }

            try
            {
                var inventario = await _dbContext.ProcInventarios.FindAsync(id);

                if (inventario == null)
                {
                    throw new EntityNotFoundException($"No se encontró ningún inventario con el ID {id}");
                }

                return new InventarioEntity
                (
                    inventario.Idestacion,
                    string.IsNullOrEmpty(inventario.NoTanque) ? null : int.Parse(inventario.NoTanque),
                    inventario.ClaveProducto,
                    inventario.VolumenDisponible,
                    inventario.Temperatura,
                    inventario.Fecha
                );
            }
            catch (SqlException sqlEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de conexión SQL al buscar el inventario con ID {id}: {sqlEx.Message}", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de base de datos al buscar el inventario con ID {id}: {dbEx.Message}", dbEx);
            }
            catch (Exception ex) when (!(ex is EntityNotFoundException) && !(ex is RepositoryException))
            {
                throw new RepositoryException(
                    $"Error inesperado al buscar el inventario con ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InventarioEntity>> GetAsync(Expression<Func<ProcInventarioModel, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "El predicado de búsqueda no puede ser nulo");
            }

            try
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
            catch (SqlException sqlEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de conexión SQL al buscar inventarios: {sqlEx.Message}", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de base de datos al buscar inventarios: {dbEx.Message}", dbEx);
            }
            catch (Exception ex) when (!(ex is RepositoryException))
            {
                throw new RepositoryException(
                    $"Error inesperado al buscar inventarios: {ex.Message}", ex);
            }
        }
    }
}
