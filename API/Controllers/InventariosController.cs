using Domain.Entities;
using Application.UseCases;
using Infrastructure.Dtos;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventariosController : ControllerBase
    {
        private readonly GetLatestInventarioByStationUseCase<ProcInventarioModel> _getLatestInventarioByStationUseCase;
        private readonly GetInventarioByIdUseCase _getInventarioByIdUseCase;
        private readonly CreateInventarioUseCase<InventarioRequestDTO> _createInventarioUseCase;
        private readonly ILogger<InventariosController> _logger;

        public InventariosController(
            GetLatestInventarioByStationUseCase<ProcInventarioModel> getLatestInventarioByStationUseCase,
            GetInventarioByIdUseCase getInventarioByIdUseCase,
            CreateInventarioUseCase<InventarioRequestDTO> createInventarioUseCase,
            ILogger<InventariosController> logger)
        {
            _getLatestInventarioByStationUseCase = getLatestInventarioByStationUseCase;
            _getInventarioByIdUseCase = getInventarioByIdUseCase;
            _createInventarioUseCase = createInventarioUseCase;
            _logger = logger;
        }

        /// <summary>
        /// Get latest inventory by station ID
        /// </summary>
        /// <param name="idEstacion">The ID of the station</param>
        /// <returns>The latest inventory records for the specified station</returns>
        /// <response code="200">Returns the latest inventory records</response>
        /// <response code="204">If no inventory records exist for the station</response>
        /// <response code="400">If the station ID is invalid</response>
        /// <response code="500">If there was an error processing the request</response>
        /// <response code="503">If there was a database connection error</response>
        [HttpGet("{idEstacion:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<IEnumerable<InventarioEntity>>> GetByEstacion(int idEstacion)
        {
            if (idEstacion <= 0)
            {
                _logger.LogWarning("Invalid station ID provided: {IdEstacion}", idEstacion);
                return BadRequest(new { message = "El ID de estación proporcionado no es válido" });
            }

            // Let the global exception middleware handle any exceptions
            var result = await _getLatestInventarioByStationUseCase.ExecuteAsync(s => s.Idestacion == idEstacion);
            
            if (result == null || !result.Any())
            {
                _logger.LogInformation("No inventory records found for station {IdEstacion}", idEstacion);
                return NoContent();
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Get inventory by ID
        /// </summary>
        /// <param name="id">The ID of the inventory record</param>
        /// <returns>The inventory record with the specified ID</returns>
        /// <response code="200">Returns the inventory record</response>
        /// <response code="400">If the ID is invalid</response>
        /// <response code="404">If the inventory record was not found</response>
        /// <response code="500">If there was an error processing the request</response>
        /// <response code="503">If there was a database connection error</response>
        [HttpGet("id/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<InventarioEntity>> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid inventory ID provided: {Id}", id);
                return BadRequest(new { message = "El ID proporcionado no es válido" });
            }

            // Si la entidad no se encuentra, el método GetByIdAsync debería lanzar EntityNotFoundException,
            // que será manejada por el middleware global de excepciones
            var result = await _getInventarioByIdUseCase.ExecuteAsync(id);
            
            // Si llegamos aquí y el resultado es nulo, significa que GetByIdAsync no lanzó una excepción
            // como se esperaba para un registro no encontrado.
            if (result == null)
            {
                _logger.LogInformation("Inventory with ID {Id} not found", id);
                return NotFound(new { message = $"No se encontró un inventario con el ID {id}" });
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Create a new inventory record
        /// </summary>
        /// <param name="inventarioRequest">The inventory data</param>
        /// <returns>A response indicating success or failure</returns>
        /// <response code="201">If the inventory record was created successfully</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="409">If there is a conflict with an existing record</response>
        /// <response code="500">If there was an error processing the request</response>
        /// <response code="503">If there was a database connection error</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> Create([FromBody] InventarioRequestDTO inventarioRequest)
        {
            if (inventarioRequest == null)
            {
                return BadRequest(new { message = "El cuerpo de la solicitud no puede estar vacío" });
            }

            // Validate required fields
            var validationErrors = new List<string>();
            
            if (inventarioRequest.IdEstacion <= 0)
                validationErrors.Add("El ID de estación es obligatorio y debe ser positivo");
                
            if (inventarioRequest.NoTanque <= 0)
                validationErrors.Add("El número de tanque es obligatorio y debe ser positivo");
                
            // Validation for other required fields based on your actual DTO structure
            if (inventarioRequest.VolumenDisponible <= 0)
                validationErrors.Add("El volumen disponible debe ser un valor positivo");
                
            if (validationErrors.Any())
            {
                return BadRequest(new { message = "Datos de inventario inválidos", errors = validationErrors });
            }

            // Cualquier excepción será manejada por el middleware global
            await _createInventarioUseCase.ExecuteAsync(inventarioRequest);
            
            _logger.LogInformation(
                "Inventory created successfully for Estacion {IdEstacion} and Tanque {NoTanque}",
                inventarioRequest.IdEstacion, inventarioRequest.NoTanque);
            
            return Created($"/api/inventarios", new { 
                message = "Inventario creado con éxito", 
                idEstacion = inventarioRequest.IdEstacion,
                noTanque = inventarioRequest.NoTanque
            });
        }
    }
}