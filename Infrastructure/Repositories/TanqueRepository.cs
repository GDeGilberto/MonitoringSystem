using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TanqueRepository : IRepository<TanqueEntity>, ITanqueRepository
    {
        private readonly AppDbContext _dbContext;

        public TanqueRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<TanqueEntity?> GetTanqueByEstacionAndNumeroAsync(int idEstacion, int noTanque)
        {
            var noTanqueStr = noTanque < 10 ? $"0{noTanque}" : noTanque.ToString();

            var tanque = await _dbContext.CatTanques
                .FirstOrDefaultAsync(t => t.IdEstacion == idEstacion && t.NoTanque == noTanqueStr);

            if (tanque == null)
                return null;

            return new TanqueEntity(
                tanque.IdEstacion,
                tanque.NoTanque,
                tanque.Producto,
                tanque.Fondeja,
                tanque.Capacidad,
                tanque.Capacidad95
            );
        }

        public async Task AddAsync(TanqueEntity entity)
        {
            // Implementación básica - normalmente los tanques no se crean desde la aplicación
            throw new NotImplementedException("La creación de tanques no está implementada");
        }

        public async Task<IEnumerable<TanqueEntity>> GetAllAsync()
        {
            var tanques = await _dbContext.CatTanques
                .Select(t => new TanqueEntity(
                    t.IdEstacion,
                    t.NoTanque,
                    t.Producto,
                    t.Fondeja,
                    t.Capacidad,
                    t.Capacidad95
                ))
                .ToListAsync();

            return tanques;
        }

        public async Task<TanqueEntity> GetByIdAsync(int id)
        {
            // Para tanques, el ID no es un solo campo, así que esta implementación es básica
            throw new NotImplementedException("GetByIdAsync no aplicable para tanques");
        }
    }
}