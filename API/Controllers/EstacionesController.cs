using Domain.Entities;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        /// Get all stations
        /// </summary>
        /// <returns>A list of all stations</returns>
        /// <response code="200">Returns the list of stations</response>
        /// <response code="204">If no stations exist</response>
        /// <response code="500">If there was an error processing the request</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<EstacionesEntity>>> Get()
        {
            try
            {
                var result = await _getEstacionesUseCase.ExecuteAsync();
                
                if (result == null || !result.Any())
                {
                    _logger.LogInformation("No stations found");
                    return NoContent();
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all stations");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Ocurrió un error al procesar la solicitud", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a station by ID
        /// </summary>
        /// <param name="id">The ID of the station</param>
        /// <returns>The requested station</returns>
        /// <response code="200">Returns the requested station</response>
        /// <response code="400">If the ID is invalid</response>
        /// <response code="404">If the station was not found</response>
        /// <response code="500">If there was an error processing the request</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EstacionesEntity>> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid station ID provided: {Id}", id);
                return BadRequest(new { message = "El ID de estación proporcionado no es válido" });
            }

            try
            {
                var result = await _getEstacionesByIdUseCase.ExecuteAsync(id);
                
                if (result == null)
                {
                    _logger.LogInformation("Station with ID {Id} not found", id);
                    return NotFound(new { message = $"No se encontró una estación con el ID {id}" });
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving station with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Ocurrió un error al procesar la solicitud", error = ex.Message });
            }
        }
    }
}