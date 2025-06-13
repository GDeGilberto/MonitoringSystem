using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Custom repository exceptions for better error handling
    /// </summary>
    public class RepositoryException : Exception 
    {
        public RepositoryException(string message) : base(message) { }
        public RepositoryException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class EntityNotFoundException : RepositoryException 
    {
        public EntityNotFoundException(string message) : base(message) { }
    }

    public class DatabaseConnectionException : RepositoryException 
    {
        public DatabaseConnectionException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class DuplicateEntityException : RepositoryException 
    {
        public DuplicateEntityException(string message) : base(message) { }
    }

    public class DescargasRepository : IRepository<DescargasEntity>, IRepositorySearch<ProcDescargaModel, DescargasEntity>
    {
        private readonly AppDbContext _dbContext;
        
        public DescargasRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddAsync(DescargasEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "La entidad de descarga no puede ser nula");
            }

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
                
                // Verificar si ya existe un registro para esta fecha
                var isRegistered = await _dbContext.ProcDescargas
                    .FirstOrDefaultAsync(d => d.IdEstacion == descarga.IdEstacion
                        && d.NoTanque == descarga.NoTanque
                        && d.FechaInicio.Date == descarga.FechaInicio.Date);

                if (isRegistered != null)
                {
                    throw new DuplicateEntityException(
                        $"Ya existe un registro de descarga para la estación {entity.IdEstacion}, " +
                        $"tanque {entity.NoTanque} en la fecha {entity.FechaInicial.ToShortDateString()}");
                }

                await _dbContext.ProcDescargas.AddAsync(descarga);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new DatabaseConnectionException(
                    $"Error al guardar la descarga en la base de datos: {dbEx.Message}", dbEx);
            }
            catch (SqlException sqlEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de SQL al agregar la descarga: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex) when (!(ex is RepositoryException))
            {
                throw new RepositoryException(
                    $"Error inesperado al agregar la descarga: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<DescargasEntity>> GetAllAsync()
        {
            try
            {
                var result = await _dbContext.ProcDescargas
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
                    ))
                    .ToListAsync();

                return result;
            }
            catch (SqlException sqlEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de conexión SQL al obtener todas las descargas: {sqlEx.Message}", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de base de datos al obtener todas las descargas: {dbEx.Message}", dbEx);
            }
            catch (Exception ex) when (!(ex is RepositoryException))
            {
                throw new RepositoryException(
                    $"Error inesperado al obtener todas las descargas: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<DescargasEntity>> GetAsync(Expression<Func<ProcDescargaModel, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "El predicado de búsqueda no puede ser nulo");
            }
            
            try
            {
                var descargasModel = await _dbContext.ProcDescargas
                    .Where(predicate)
                    .ToListAsync();

                if (descargasModel == null || !descargasModel.Any())
                {
                    return Enumerable.Empty<DescargasEntity>();
                }

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
            catch (SqlException sqlEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de conexión SQL al buscar descargas: {sqlEx.Message}", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de base de datos al buscar descargas: {dbEx.Message}", dbEx);
            }
            catch (Exception ex) when (!(ex is RepositoryException))
            {
                throw new RepositoryException(
                    $"Error inesperado al buscar descargas: {ex.Message}", ex);
            }
        }

        public async Task<DescargasEntity?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser un valor positivo", nameof(id));
            }
            
            try
            {
                var descargaModel = await _dbContext.ProcDescargas.FindAsync(id);
                
                if (descargaModel == null)
                {
                    throw new EntityNotFoundException($"No se encontró ninguna descarga con el ID {id}");
                }
                
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
            catch (SqlException sqlEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de conexión SQL al buscar la descarga con ID {id}: {sqlEx.Message}", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de base de datos al buscar la descarga con ID {id}: {dbEx.Message}", dbEx);
            }
            catch (Exception ex) when (!(ex is EntityNotFoundException) && !(ex is RepositoryException))
            {
                throw new RepositoryException(
                    $"Error inesperado al buscar la descarga con ID {id}: {ex.Message}", ex);
            }
        }
    }
}
