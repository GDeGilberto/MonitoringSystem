using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories.Demo
{
    public class DemoEstacionesRepository : IRepository<EstacionesEntity>
    {
        private static readonly List<EstacionesEntity> _estaciones = new()
        {
            new EstacionesEntity(
                id: 11162,
                nombre: "Estación Demo Gasolinera Central",
                direccion: "Av. Principal 123, Ciudad Demo",
                telefono: "555-0123",
                nomContacto: "Contacto Demo",
                correoElectronico: "demo@gasolinera.com",
                activa: "S",
                ultimoEnvio: DateTime.Now.AddMinutes(-30),
                envioCorreo: DateTime.Now.AddHours(-1),
                distUbicacion: "Centro",
                tipoCliente: "Gasolinera",
                idCliente: 1,
                atiende: "Auto",
                idZonaPrecio: 1,
                ultimaActualizacionAnalisis: DateTime.Now.AddHours(-2),
                idEstacionAutoabasto: null,
                tanques: new List<TanquesEntity>
                {
                    new TanquesEntity(11162, "1", "34006", 100, 15000, 14250, null),
                    new TanquesEntity(11162, "2", "34007", 100, 12000, 11400, null),
                    new TanquesEntity(11162, "3", "34008", 100, 18000, 17100, null),
                    new TanquesEntity(11162, "4", "34006", 100, 15000, 14250, null),
                    new TanquesEntity(11162, "5", "34006", 100, 11000, 11400, null)
                }
            )
        };

        public Task AddAsync(EstacionesEntity entity)
        {
            var newEstacion = new EstacionesEntity(
                id: _estaciones.Count + 1,
                nombre: entity.Nombre,
                direccion: entity.Direccion,
                telefono: entity.Telefono,
                nomContacto: entity.NomContacto,
                correoElectronico: entity.CorreoElectronico,
                activa: entity.Activa,
                ultimoEnvio: entity.UltimoEnvio,
                envioCorreo: entity.EnvioCorreo,
                distUbicacion: entity.DistUbicacion,
                tipoCliente: entity.TipoCliente,
                idCliente: entity.IdCliente,
                atiende: entity.Atiende,
                idZonaPrecio: entity.IdZonaPrecio,
                ultimaActualizacionAnalisis: entity.UltimaActualizacionAnalisis,
                idEstacionAutoabasto: entity.IdEstacionAutoabasto,
                tanques: entity.Tanques
            );
            
            _estaciones.Add(newEstacion);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<EstacionesEntity>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<EstacionesEntity>>(_estaciones);
        }

        public Task<EstacionesEntity> GetByIdAsync(int id)
        {
            var estacion = _estaciones.FirstOrDefault(e => e.Id == id);
            if (estacion == null)
                throw new KeyNotFoundException($"Estación with ID {id} not found");

            return Task.FromResult(estacion);
        }

        public Task UpdateAsync(EstacionesEntity entity)
        {
            var existingIndex = _estaciones.FindIndex(e => e.Id == entity.Id);
            if (existingIndex >= 0)
            {
                _estaciones[existingIndex] = entity;
            }
            
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            var existing = _estaciones.FirstOrDefault(e => e.Id == id);
            if (existing != null)
            {
                _estaciones.Remove(existing);
            }
            
            return Task.CompletedTask;
        }
    }
}