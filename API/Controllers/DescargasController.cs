using Application.UseCases;
using Domain.Entities;
using Infrastructure.Dtos;
using Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para la gesti�n de descargas de combustible
    /// </summary>
    /// <remarks>
    /// Este controlador permite realizar operaciones CRUD sobre los registros de descargas en estaciones.
    /// Las descargas representan el proceso de transferencia de combustible a un tanque espec�fico.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class DescargasController : ControllerBase
    {
        private readonly GetDescargasUseCase _getDescargasUseCase;
        private readonly GetDescargaByIdUseCase _getDescargaByIdUseCase;
        private readonly GetDescargaSearchUseCase<ProcDescargaModel> _getDescargaSearchUseCase;
        private readonly CreateDescargasUseCase<DescargaRequestDTO> _createDescargasUseCase;
        private readonly ILogger<DescargasController> _logger;

        public DescargasController(
            GetDescargasUseCase getDescargasUseCase,
            GetDescargaByIdUseCase getDescargaByIdUseCase,
            GetDescargaSearchUseCase<ProcDescargaModel> getDescargaSearchUseCase,
            CreateDescargasUseCase<DescargaRequestDTO> createDescargasUseCase,
            ILogger<DescargasController> logger)
        {
            _getDescargasUseCase = getDescargasUseCase;
            _getDescargaByIdUseCase = getDescargaByIdUseCase;
            _getDescargaSearchUseCase = getDescargaSearchUseCase;
            _createDescargasUseCase = createDescargasUseCase;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los registros de descargas
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna un listado completo de todas las descargas registradas en el sistema.
        /// Si no existen registros, se devuelve un c�digo 204 No Content.
        /// 
        /// Los registros est�n ordenados por fecha de descarga, del m�s reciente al m�s antiguo.
        /// </remarks>
        /// <returns>Lista de todos los registros de descargas</returns>
        /// <response code="200">Retorna la lista de descargas</response>
        /// <response code="204">Si no existen registros de descargas</response>
        /// <response code="401">Si el usuario no est� autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para acceder a este recurso</response>
        /// <response code="500">Si ocurri� un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexi�n con la base de datos</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtiene todos los registros de descargas",
            Description = "Retorna la lista completa de descargas en el sistema",
            OperationId = "GetDescargas",
            Tags = new[] { "Descargas" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<IEnumerable<DescargasEntity>>> Get()
        {
            var result = await _getDescargasUseCase.ExecuteAsync();
            
            if (result == null || !result.Any())
            {
                _logger.LogInformation("No descargas records found");
                return NoContent();
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una descarga por su ID
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna informaci�n detallada de una descarga espec�fica seg�n su ID.
        /// 
        /// Si el ID no es v�lido (menor o igual a cero), se retorna BadRequest.
        /// Si no se encuentra una descarga con el ID especificado, se retorna NotFound.
        /// </remarks>
        /// <param name="id">ID de la descarga a consultar</param>
        /// <returns>La descarga solicitada</returns>
        /// <response code="200">Retorna la descarga solicitada</response>
        /// <response code="400">Si el ID proporcionado no es v�lido</response>
        /// <response code="401">Si el usuario no est� autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para acceder a este recurso</response>
        /// <response code="404">Si no se encontr� la descarga</response>
        /// <response code="500">Si ocurri� un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexi�n con la base de datos</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obtiene una descarga por su ID",
            Description = "Retorna informaci�n detallada de una descarga espec�fica",
            OperationId = "GetDescargaById",
            Tags = new[] { "Descargas" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<DescargasEntity>> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid descarga ID provided: {Id}", id);
                return BadRequest(new { message = "El ID proporcionado no es v�lido" });
            }

            var result = await _getDescargaByIdUseCase.ExecuteAsync(id);
            
            // If we reach here, the entity was found (otherwise an EntityNotFoundException would have been thrown)
            _logger.LogInformation("Descarga with ID {Id} retrieved successfully", id);
            return Ok(result);
        }

        /// <summary>
        /// Busca descargas por ID de estaci�n y n�mero de tanque
        /// </summary>
        /// <remarks>
        /// Este endpoint permite filtrar descargas por una combinaci�n de ID de estaci�n y n�mero de tanque.
        /// 
        /// Ambos par�metros son obligatorios y deben ser valores positivos.
        /// Si no se encuentran descargas que coincidan con los criterios, se retorna NotFound.
        /// </remarks>
        /// <param name="idEstacion">ID de la estaci�n (requerido, debe ser positivo)</param>
        /// <param name="noTanque">N�mero del tanque (requerido, debe ser positivo)</param>
        /// <returns>Lista de descargas que coinciden con los criterios de b�squeda</returns>
        /// <response code="200">Retorna la lista de descargas que coinciden con los criterios</response>
        /// <response code="400">Si alguno de los par�metros no es v�lido</response>
        /// <response code="401">Si el usuario no est� autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para acceder a este recurso</response>
        /// <response code="404">Si no se encontraron descargas que coincidan con los criterios</response>
        /// <response code="500">Si ocurri� un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexi�n con la base de datos</response>
        [HttpGet("search")]
        [SwaggerOperation(
            Summary = "Busca descargas por estaci�n y tanque",
            Description = "Filtra descargas seg�n el ID de estaci�n y n�mero de tanque",
            OperationId = "SearchDescargas",
            Tags = new[] { "Descargas" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<IEnumerable<DescargasEntity>>> GetByEstacionAndTanque(
            [FromQuery][Required] int? idEstacion, 
            [FromQuery][Required] int? noTanque)
        {
            if (!idEstacion.HasValue || !noTanque.HasValue)
            {
                return BadRequest(new { 
                    message = "Se requieren tanto el ID de estaci�n como el n�mero de tanque" 
                });
            }

            if (idEstacion <= 0 || noTanque <= 0)
            {
                return BadRequest(new { 
                    message = "El ID de estaci�n y n�mero de tanque deben ser valores positivos" 
                });
            }

            var result = await _getDescargaSearchUseCase.ExecuteAsync(
                d => d.IdEstacion == idEstacion.Value && d.NoTanque == noTanque.Value);
            
            if (result == null || !result.Any())
            {
                _logger.LogInformation("No descargas found for Estacion {IdEstacion} and Tanque {NoTanque}", 
                    idEstacion, noTanque);
                return NotFound(new { 
                    message = $"No se encontraron descargas para la estaci�n {idEstacion} y tanque {noTanque}" 
                });
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo registro de descarga
        /// </summary>
        /// <remarks>
        /// Este endpoint permite registrar una nueva descarga en el sistema.
        /// 
        /// **Campos requeridos:**
        /// - IdEstacion: ID de la estaci�n (valor positivo)
        /// - NoTanque: N�mero del tanque (valor positivo)
        /// - VolumenInicial: Volumen inicial (en litros, valor positivo)
        /// - FechaInicial: Fecha y hora de inicio de la descarga
        /// - VolumenDisponible: Volumen disponible despu�s de la descarga (en litros, valor positivo)
        /// - FechaFinal: Fecha y hora de finalizaci�n de la descarga
        /// 
        /// Ejemplo de solicitud:
        /// 
        /// ```json
        /// {
        ///   "idEstacion": 1,
        ///   "noTanque": 2,
        ///   "volumenInicial": 1000.5,
        ///   "fechaInicial": "2023-06-15T08:30:00Z",
        ///   "volumenDisponible": 3500.75,
        ///   "fechaFinal": "2023-06-15T09:45:00Z",
        ///   "observaciones": "Descarga regular programada"
        /// }
        /// ```
        /// </remarks>
        /// <param name="descargaRequest">Datos de la descarga a crear</param>
        /// <returns>Respuesta indicando el �xito o fracaso de la operaci�n</returns>
        /// <response code="201">Si la descarga se cre� exitosamente</response>
        /// <response code="400">Si los datos de la solicitud son inv�lidos</response>
        /// <response code="401">Si el usuario no est� autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para realizar esta acci�n</response>
        /// <response code="409">Si hay un conflicto con un registro existente</response>
        /// <response code="500">Si ocurri� un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexi�n con la base de datos</response>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Crea un nuevo registro de descarga",
            Description = "Registra una nueva descarga en el sistema",
            OperationId = "CreateDescarga",
            Tags = new[] { "Descargas" }
        )]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> Create([FromBody] DescargaRequestDTO descargaRequest)
        {
            if (descargaRequest == null)
            {
                return BadRequest(new { message = "El cuerpo de la solicitud no puede estar vac�o" });
            }

            // Validate required fields
            var validationErrors = new List<string>();
            
            if (!descargaRequest.IdEstacion.HasValue || descargaRequest.IdEstacion <= 0)
                validationErrors.Add("El ID de estaci�n es obligatorio y debe ser positivo");
                
            if (!descargaRequest.NoTanque.HasValue || descargaRequest.NoTanque <= 0)
                validationErrors.Add("El n�mero de tanque es obligatorio y debe ser positivo");
                
            if (!descargaRequest.VolumenInicial.HasValue || descargaRequest.VolumenInicial <= 0)
                validationErrors.Add("El volumen inicial es obligatorio y debe ser positivo");
                
            if (!descargaRequest.FechaInicial.HasValue)
                validationErrors.Add("La fecha inicial es obligatoria");
                
            if (!descargaRequest.VolumenDisponible.HasValue || descargaRequest.VolumenDisponible <= 0)
                validationErrors.Add("El volumen disponible es obligatorio y debe ser positivo");
                
            if (!descargaRequest.FechaFinal.HasValue)
                validationErrors.Add("La fecha final es obligatoria");
            
            if (validationErrors.Any())
            {
                return BadRequest(new { message = "Datos de descarga inv�lidos", errors = validationErrors });
            }

            // Let any exceptions bubble up to the global exception handler middleware
            await _createDescargasUseCase.ExecuteAsync(descargaRequest);
            
            _logger.LogInformation(
                "Descarga created successfully for Estacion {IdEstacion} and Tanque {NoTanque}",
                descargaRequest.IdEstacion, descargaRequest.NoTanque);
            
            // Since we don't know the generated ID, we return the resource location as a general endpoint
            return Created($"/api/descargas", new { 
                message = "Descarga creada con �xito", 
                idEstacion = descargaRequest.IdEstacion,
                noTanque = descargaRequest.NoTanque
            });
        }
    }
}