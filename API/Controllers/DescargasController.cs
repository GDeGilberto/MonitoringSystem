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
    /// Controlador para la gestión de descargas de combustible
    /// </summary>
    /// <remarks>
    /// Este controlador permite realizar operaciones CRUD sobre los registros de descargas en estaciones.
    /// Las descargas representan el proceso de transferencia de combustible a un tanque específico.
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
        /// Si no existen registros, se devuelve un código 204 No Content.
        /// 
        /// Los registros están ordenados por fecha de descarga, del más reciente al más antiguo.
        /// </remarks>
        /// <returns>Lista de todos los registros de descargas</returns>
        /// <response code="200">Retorna la lista de descargas</response>
        /// <response code="204">Si no existen registros de descargas</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para acceder a este recurso</response>
        /// <response code="500">Si ocurrió un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexión con la base de datos</response>
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
        /// Este endpoint retorna información detallada de una descarga específica según su ID.
        /// 
        /// Si el ID no es válido (menor o igual a cero), se retorna BadRequest.
        /// Si no se encuentra una descarga con el ID especificado, se retorna NotFound.
        /// </remarks>
        /// <param name="id">ID de la descarga a consultar</param>
        /// <returns>La descarga solicitada</returns>
        /// <response code="200">Retorna la descarga solicitada</response>
        /// <response code="400">Si el ID proporcionado no es válido</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para acceder a este recurso</response>
        /// <response code="404">Si no se encontró la descarga</response>
        /// <response code="500">Si ocurrió un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexión con la base de datos</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obtiene una descarga por su ID",
            Description = "Retorna información detallada de una descarga específica",
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
                return BadRequest(new { message = "El ID proporcionado no es válido" });
            }

            var result = await _getDescargaByIdUseCase.ExecuteAsync(id);
            
            // If we reach here, the entity was found (otherwise an EntityNotFoundException would have been thrown)
            _logger.LogInformation("Descarga with ID {Id} retrieved successfully", id);
            return Ok(result);
        }

        /// <summary>
        /// Busca descargas por ID de estación y número de tanque
        /// </summary>
        /// <remarks>
        /// Este endpoint permite filtrar descargas por una combinación de ID de estación y número de tanque.
        /// 
        /// Ambos parámetros son obligatorios y deben ser valores positivos.
        /// Si no se encuentran descargas que coincidan con los criterios, se retorna NotFound.
        /// </remarks>
        /// <param name="idEstacion">ID de la estación (requerido, debe ser positivo)</param>
        /// <param name="noTanque">Número del tanque (requerido, debe ser positivo)</param>
        /// <returns>Lista de descargas que coinciden con los criterios de búsqueda</returns>
        /// <response code="200">Retorna la lista de descargas que coinciden con los criterios</response>
        /// <response code="400">Si alguno de los parámetros no es válido</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para acceder a este recurso</response>
        /// <response code="404">Si no se encontraron descargas que coincidan con los criterios</response>
        /// <response code="500">Si ocurrió un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexión con la base de datos</response>
        [HttpGet("search")]
        [SwaggerOperation(
            Summary = "Busca descargas por estación y tanque",
            Description = "Filtra descargas según el ID de estación y número de tanque",
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
                    message = "Se requieren tanto el ID de estación como el número de tanque" 
                });
            }

            if (idEstacion <= 0 || noTanque <= 0)
            {
                return BadRequest(new { 
                    message = "El ID de estación y número de tanque deben ser valores positivos" 
                });
            }

            var result = await _getDescargaSearchUseCase.ExecuteAsync(
                d => d.IdEstacion == idEstacion.Value && d.NoTanque == noTanque.Value);
            
            if (result == null || !result.Any())
            {
                _logger.LogInformation("No descargas found for Estacion {IdEstacion} and Tanque {NoTanque}", 
                    idEstacion, noTanque);
                return NotFound(new { 
                    message = $"No se encontraron descargas para la estación {idEstacion} y tanque {noTanque}" 
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
        /// - IdEstacion: ID de la estación (valor positivo)
        /// - NoTanque: Número del tanque (valor positivo)
        /// - VolumenInicial: Volumen inicial (en litros, valor positivo)
        /// - FechaInicial: Fecha y hora de inicio de la descarga
        /// - VolumenDisponible: Volumen disponible después de la descarga (en litros, valor positivo)
        /// - FechaFinal: Fecha y hora de finalización de la descarga
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
        /// <returns>Respuesta indicando el éxito o fracaso de la operación</returns>
        /// <response code="201">Si la descarga se creó exitosamente</response>
        /// <response code="400">Si los datos de la solicitud son inválidos</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para realizar esta acción</response>
        /// <response code="409">Si hay un conflicto con un registro existente</response>
        /// <response code="500">Si ocurrió un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexión con la base de datos</response>
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
                return BadRequest(new { message = "El cuerpo de la solicitud no puede estar vacío" });
            }

            // Validate required fields
            var validationErrors = new List<string>();
            
            if (!descargaRequest.IdEstacion.HasValue || descargaRequest.IdEstacion <= 0)
                validationErrors.Add("El ID de estación es obligatorio y debe ser positivo");
                
            if (!descargaRequest.NoTanque.HasValue || descargaRequest.NoTanque <= 0)
                validationErrors.Add("El número de tanque es obligatorio y debe ser positivo");
                
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
                return BadRequest(new { message = "Datos de descarga inválidos", errors = validationErrors });
            }

            // Let any exceptions bubble up to the global exception handler middleware
            await _createDescargasUseCase.ExecuteAsync(descargaRequest);
            
            _logger.LogInformation(
                "Descarga created successfully for Estacion {IdEstacion} and Tanque {NoTanque}",
                descargaRequest.IdEstacion, descargaRequest.NoTanque);
            
            // Since we don't know the generated ID, we return the resource location as a general endpoint
            return Created($"/api/descargas", new { 
                message = "Descarga creada con éxito", 
                idEstacion = descargaRequest.IdEstacion,
                noTanque = descargaRequest.NoTanque
            });
        }
    }
}