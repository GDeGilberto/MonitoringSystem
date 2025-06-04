using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases
{
    public class CreateDescargasUseCase<TDTO>
    {
        public IRepository<DescargasEntity> _repository;
        public IMapper<TDTO, DescargasEntity> _mapper;

        public CreateDescargasUseCase(IRepository<DescargasEntity> repository, 
            IMapper<TDTO, DescargasEntity> mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task ExecuteAsync(TDTO descargaDTO)
        {
            var descarga = _mapper.ToEntity(descargaDTO);

            if (descarga.IdEstacion <= 0)
            {
                throw new ArgumentException("Debe tener un id de estacion");
            }

            if (descarga.NoTanque <= 0)
            {
                throw new ArgumentException("Debe tener un numero de tanque");
            }

            if (descarga.VolumenInicial <= 0)
            {
                throw new ArgumentException("Debe tener un volumen inicial mayor a 0");
            }

            if (descarga.FechaInicial == default)
            {
                throw new ArgumentException("Debe tener una fecha inicial válida");
            }

            if (descarga.VolumenDisponible <= 0)
            {
                throw new ArgumentException("Debe tener un volumen disponible mayor a 0");
            }

            if (descarga.FechaFinal == default)
            {
                throw new ArgumentException("Debe tener una fecha final válida");
            }

            await _repository.AddAsync(descarga);
        }
    }
}
