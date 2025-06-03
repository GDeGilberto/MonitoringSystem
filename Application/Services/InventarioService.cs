using Application.Interfaces;

namespace Application.Services
{
    public class InventarioService<TEntity, TOutput>
    {
        private readonly IRepository<TEntity> _repository;
        private readonly IPresenter<TEntity, TOutput> _presenter;
        public InventarioService(
            IRepository<TEntity> repository, 
            IPresenter<TEntity, TOutput> presenter)
        {
            _repository = repository;
            _presenter = presenter;
        }

        public async Task<IEnumerable<TOutput>> GetAllAsync()
        {
            var inventarios = await _repository.GetAllAsync();
            return _presenter.Present(inventarios);
        }

        public async Task AddAsync(TEntity inventario)
        {
            await _repository.AddAsync(inventario);
        }

        //public async Task<TOutput> GetByIdAsync(int id)
        //{
        //    var inventario = await _repository.GetByIdAsync(id);
        //    return _presenter.Present(inventario);
        //}
    }
}
