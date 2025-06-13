using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class EstacionesRepository : IRepository<EstacionesEntity>
    {
        private readonly AppDbContext _dbContext;

        public EstacionesRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public Task AddAsync(EstacionesEntity entity)
        {
            throw new NotImplementedException("La función de agregar estaciones no está implementada");
        }

        public async Task<IEnumerable<EstacionesEntity>> GetAllAsync()
        {
            try
            {
                var estaciones = await _dbContext.CatEstaciones
                    .Include(e => e.CatTanques)
                    .ToListAsync();

                if (estaciones == null || !estaciones.Any())
                {
                    // No es un error, simplemente no hay estaciones
                    return Enumerable.Empty<EstacionesEntity>();
                }

                return estaciones.Select(e => MapToEntity(e));
            }
            catch (SqlException sqlEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de conexión SQL al obtener todas las estaciones: {sqlEx.Message}", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de base de datos al obtener todas las estaciones: {dbEx.Message}", dbEx);
            }
            catch (Exception ex) when (!(ex is RepositoryException))
            {
                throw new RepositoryException(
                    $"Error inesperado al obtener todas las estaciones: {ex.Message}", ex);
            }
        }

        public async Task<EstacionesEntity> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID de estación debe ser un valor positivo", nameof(id));
            }

            try
            {
                var estacionesModel = await _dbContext.CatEstaciones
                    .Include(e => e.CatTanques)
                    .FirstOrDefaultAsync(e => e.IdEstacion == id);

                if (estacionesModel == null)
                {
                    throw new EntityNotFoundException($"No se encontró ninguna estación con el ID {id}");
                }

                return MapToEntity(estacionesModel);
            }
            catch (SqlException sqlEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de conexión SQL al buscar la estación con ID {id}: {sqlEx.Message}", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new DatabaseConnectionException(
                    $"Error de base de datos al buscar la estación con ID {id}: {dbEx.Message}", dbEx);
            }
            catch (Exception ex) when (!(ex is EntityNotFoundException) && !(ex is RepositoryException))
            {
                throw new RepositoryException(
                    $"Error inesperado al buscar la estación con ID {id}: {ex.Message}", ex);
            }
        }

        private static EstacionesEntity MapToEntity(Models.CatEstacionesModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "El modelo de estación no puede ser nulo");
            }

            return new EstacionesEntity(
                model.IdEstacion,
                model.Nombre,
                model.Direccion,
                model.Telefono,
                model.NomContacto,
                model.CorreoElectronico,
                model.Activa,
                model.UltimoEnvio,
                model.EnvioCorreo,
                model.DistUbicacion,
                model.TipoCliente,
                model.Idcliente,
                model.Atiende,
                model.IdZonaPrecio,
                model.UltimaActualizacionAnalisis,
                model.IdEstacionAutoabasto,
                model.CatTanques.Select(t => new TanquesEntity(
                    t.IdEstacion,
                    t.NoTanque,
                    t.Producto,
                    t.Fondeja,
                    t.Capacidad,
                    t.Capacidad95,
                    null
                )).ToList()
            );
        }
    }
}
