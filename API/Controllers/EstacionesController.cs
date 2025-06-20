using Application.UseCases;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de estaciones de monitoreo
    /// </summary>
    /// <remarks>
    /// Este controlador permite realizar operaciones sobre las estaciones de monitoreo.
    /// Las estaciones son puntos físicos donde se encuentran los tanques de combustible.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class EstacionesController : ControllerBase
    {
        private readonly GetEstacionesUseCase _getEstacionesUseCase;
        private readonly GetEstacionesByIdUseCase _getEstacionesByIdUseCase;
        private readonly ILogger<EstacionesController> _logger;

        public EstacionesController(
            GetEstacionesUseCase getEstacionesUseCase,
            GetEstacionesByIdUseCase getEstacionesByIdUseCase,
            ILogger<EstacionesController> logger)
        {
            _getEstacionesUseCase = getEstacionesUseCase;
            _getEstacionesByIdUseCase = getEstacionesByIdUseCase;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las estaciones de monitoreo
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna un listado completo de todas las estaciones registradas en el sistema.
        /// Si no existen estaciones, se devuelve un código 204 No Content.
        /// 
        /// Las estaciones incluyen información como:
        /// - ID único de la estación
        /// - Nombre de la estación
        /// - Ubicación geográfica
        /// - Estado operativo
        /// 
        /// El listado está ordenado alfabéticamente por nombre de estación.
        /// </remarks>
        /// <returns>Lista de todas las estaciones</returns>
        /// <response code="200">Retorna la lista de estaciones</response>
        /// <response code="204">Si no existen estaciones en el sistema</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para acceder a este recurso</response>
        /// <response code="500">Si ocurrió un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexión con la base de datos</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtiene todas las estaciones",
            Description = "Retorna el listado completo de estaciones de monitoreo",
            OperationId = "GetEstaciones",
            Tags = new[] { "Estaciones" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<IEnumerable<EstacionesEntity>>> Get()
        {
            var result = await _getEstacionesUseCase.ExecuteAsync();
            
            if (result == null || !result.Any())
            {
                _logger.LogInformation("No stations found");
                return NoContent();
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Obtiene una estación por su ID
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna información detallada de una estación específica según su ID.
        /// 
        /// Si el ID no es válido (menor o igual a cero), se retorna BadRequest.
        /// Si no se encuentra una estación con el ID especificado, se retorna NotFound.
        /// 
        /// La información de la estación incluye:
        /// - Datos básicos (nombre, ubicación)
        /// - Estado operativo actual
        /// - Fecha de último mantenimiento
        /// - Otros detalles específicos de la estación
        /// </remarks>
        /// <param name="id">ID de la estación a consultar</param>
        /// <returns>La estación solicitada</returns>
        /// <response code="200">Retorna la estación solicitada</response>
        /// <response code="400">Si el ID proporcionado no es válido</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para acceder a este recurso</response>
        /// <response code="404">Si no se encontró la estación</response>
        /// <response code="500">Si ocurrió un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexión con la base de datos</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obtiene una estación por su ID",
            Description = "Retorna información detallada de una estación específica",
            OperationId = "GetEstacionById",
            Tags = new[] { "Estaciones" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<EstacionesEntity>> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid station ID provided: {Id}", id);
                return BadRequest(new { message = "El ID de estación proporcionado no es válido" });
            }

            // Any exceptions here will be caught by the global exception handling middleware
            var result = await _getEstacionesByIdUseCase.ExecuteAsync(id);
            
            // If we reach here and result is null, it means the GetByIdAsync didn't throw an EntityNotFoundException
            // This could happen if the repository doesn't throw exceptions for not found entities
            if (result == null)
            {
                _logger.LogInformation("Station with ID {Id} not found", id);
                return NotFound(new { message = $"No se encontró una estación con el ID {id}" });
            }
            
            _logger.LogInformation("Retrieved station with ID {Id}", id);
            return Ok(result);
        }
    }
}