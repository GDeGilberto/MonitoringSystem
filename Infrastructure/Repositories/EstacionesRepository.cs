using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class EstacionesRepository : IRepository<EstacionesEntity>
    {
        private readonly AppDbContext _dbContext;

        public EstacionesRepository(AppDbContext dbContext)
            => _dbContext = dbContext;

        public Task AddAsync(EstacionesEntity entity)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<EstacionesEntity>> GetAllAsync()
        {
            var estaciones = await _dbContext.CatEstaciones
                .Include(e => e.CatTanques)
                .ToListAsync();

            return estaciones.Select(e => new EstacionesEntity(
                e.IdEstacion,
                e.Nombre,
                e.Direccion,
                e.Telefono,
                e.NomContacto,
                e.CorreoElectronico,
                e.Activa,
                e.UltimoEnvio,
                e.EnvioCorreo,
                e.DistUbicacion,
                e.TipoCliente,
                e.Idcliente,
                e.Atiende,
                e.IdZonaPrecio,
                e.UltimaActualizacionAnalisis,
                e.IdEstacionAutoabasto,
                e.CatTanques.Select(t => new TanquesEntity(
                    t.IdEstacion,
                    t.NoTanque,
                    t.Producto,
                    t.Fondeja,
                    t.Capacidad,
                    t.Capacidad95,
                    null
                )).ToList()
            ));
        }

        public async Task<EstacionesEntity> GetByIdAsync(int id)
        {
            var e = await _dbContext.CatEstaciones
                .Include(e => e.CatTanques)
                .FirstOrDefaultAsync(e => e.IdEstacion == id);

            if (e == null) return null;

            return new EstacionesEntity(
                e.IdEstacion,
                e.Nombre,
                e.Direccion,
                e.Telefono,
                e.NomContacto,
                e.CorreoElectronico,
                e.Activa,
                e.UltimoEnvio,
                e.EnvioCorreo,
                e.DistUbicacion,
                e.TipoCliente,
                e.Idcliente,
                e.Atiende,
                e.IdZonaPrecio,
                e.UltimaActualizacionAnalisis,
                e.IdEstacionAutoabasto,
                e.CatTanques.Select(t => new TanquesEntity(
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
