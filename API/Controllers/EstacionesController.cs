using Application.UseCases;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para la gesti�n de estaciones de monitoreo
    /// </summary>
    /// <remarks>
    /// Este controlador permite realizar operaciones sobre las estaciones de monitoreo.
    /// Las estaciones son puntos f�sicos donde se encuentran los tanques de combustible.
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
        /// Si no existen estaciones, se devuelve un c�digo 204 No Content.
        /// 
        /// Las estaciones incluyen informaci�n como:
        /// - ID �nico de la estaci�n
        /// - Nombre de la estaci�n
        /// - Ubicaci�n geogr�fica
        /// - Estado operativo
        /// 
        /// El listado est� ordenado alfab�ticamente por nombre de estaci�n.
        /// </remarks>
        /// <returns>Lista de todas las estaciones</returns>
        /// <response code="200">Retorna la lista de estaciones</response>
        /// <response code="204">Si no existen estaciones en el sistema</response>
        /// <response code="401">Si el usuario no est� autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para acceder a este recurso</response>
        /// <response code="500">Si ocurri� un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexi�n con la base de datos</response>
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
        /// Obtiene una estaci�n por su ID
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna informaci�n detallada de una estaci�n espec�fica seg�n su ID.
        /// 
        /// Si el ID no es v�lido (menor o igual a cero), se retorna BadRequest.
        /// Si no se encuentra una estaci�n con el ID especificado, se retorna NotFound.
        /// 
        /// La informaci�n de la estaci�n incluye:
        /// - Datos b�sicos (nombre, ubicaci�n)
        /// - Estado operativo actual
        /// - Fecha de �ltimo mantenimiento
        /// - Otros detalles espec�ficos de la estaci�n
        /// </remarks>
        /// <param name="id">ID de la estaci�n a consultar</param>
        /// <returns>La estaci�n solicitada</returns>
        /// <response code="200">Retorna la estaci�n solicitada</response>
        /// <response code="400">Si el ID proporcionado no es v�lido</response>
        /// <response code="401">Si el usuario no est� autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para acceder a este recurso</response>
        /// <response code="404">Si no se encontr� la estaci�n</response>
        /// <response code="500">Si ocurri� un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexi�n con la base de datos</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obtiene una estaci�n por su ID",
            Description = "Retorna informaci�n detallada de una estaci�n espec�fica",
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
                return BadRequest(new { message = "El ID de estaci�n proporcionado no es v�lido" });
            }

            // Any exceptions here will be caught by the global exception handling middleware
            var result = await _getEstacionesByIdUseCase.ExecuteAsync(id);
            
            // If we reach here and result is null, it means the GetByIdAsync didn't throw an EntityNotFoundException
            // This could happen if the repository doesn't throw exceptions for not found entities
            if (result == null)
            {
                _logger.LogInformation("Station with ID {Id} not found", id);
                return NotFound(new { message = $"No se encontr� una estaci�n con el ID {id}" });
            }
            
            _logger.LogInformation("Retrieved station with ID {Id}", id);
            return Ok(result);
        }
    }
}