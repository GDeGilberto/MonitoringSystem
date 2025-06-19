using Application.UseCases;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
        /// Get all stations.
        /// </summary>
        /// <returns>A list of all stations</returns>
        /// <response code="200">Returns the list of stations</response>
        /// <response code="204">If no stations exist</response>
        /// <response code="500">If there was an error processing the request</response>
        /// <response code="503">If there was a database connection error</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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
        /// Get a station by ID
        /// </summary>
        /// <param name="id">The ID of the station</param>
        /// <returns>The requested station</returns>
        /// <response code="200">Returns the requested station</response>
        /// <response code="400">If the ID is invalid</response>
        /// <response code="404">If the station was not found</response>
        /// <response code="500">If there was an error processing the request</response>
        /// <response code="503">If there was a database connection error</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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