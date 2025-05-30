using Application.Interfaces;

namespace Application.Services
{
    public class DescargasService<TEntity>
    {
        private readonly IRepository<TEntity> _repository;

        public DescargasService(IRepository<TEntity> repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(TEntity descarga)
        {
            await _repository.AddAsync(descarga);
        }
    }
}
