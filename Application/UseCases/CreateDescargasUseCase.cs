using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases
{
    public class CreateDescargasUseCase<TDTO>
    {
        private readonly IRepository<DescargasEntity> _repository;
        private readonly IMapper<TDTO, DescargasEntity> _mapper;

        public CreateDescargasUseCase(IRepository<DescargasEntity> repository, 
            IMapper<TDTO, DescargasEntity> mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Executes the use case to create a new descarga
        /// </summary>
        /// <param name="descargaDTO">The DTO containing descarga data</param>
        /// <exception cref="ArgumentNullException">Thrown when the DTO is null</exception>
        /// <exception cref="ArgumentException">Thrown when the DTO contains invalid data</exception>
        /// <exception cref="Infrastructure.Repositories.DuplicateEntityException">Thrown when a conflicting entity already exists</exception>
        /// <exception cref="Infrastructure.Repositories.DatabaseConnectionException">Thrown when a database connection error occurs</exception>
        /// <exception cref="Infrastructure.Repositories.RepositoryException">Thrown when a repository error occurs</exception>
        public async Task ExecuteAsync(TDTO descargaDTO)
        {
            if (descargaDTO == null)
            {
                throw new ArgumentNullException(nameof(descargaDTO), "Los datos de descarga no pueden ser nulos");
            }

            var descarga = _mapper.ToEntity(descargaDTO);

            // Validate entity properties
            var validationErrors = new List<string>();

            if (descarga.IdEstacion <= 0)
                validationErrors.Add("Debe tener un id de estación válido");

            if (descarga.NoTanque <= 0)
                validationErrors.Add("Debe tener un número de tanque válido");

            if (descarga.VolumenInicial <= 0)
                validationErrors.Add("Debe tener un volumen inicial mayor a 0");

            if (descarga.FechaInicial == default)
                validationErrors.Add("Debe tener una fecha inicial válida");

            if (descarga.VolumenDisponible <= 0)
                validationErrors.Add("Debe tener un volumen disponible mayor a 0");

            if (descarga.FechaFinal == default)
                validationErrors.Add("Debe tener una fecha final válida");
            
            if (validationErrors.Any())
            {
                throw new ArgumentException(string.Join(". ", validationErrors));
            }

            await _repository.AddAsync(descarga);
        }
    }
}
