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
        /// <returns>Lista de todas las estaciones</returns>
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
        /// <param name="id">ID de la estación a consultar</param>
        /// <returns>La estación solicitada</returns>
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

            var result = await _getEstacionesByIdUseCase.ExecuteAsync(id);
            
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