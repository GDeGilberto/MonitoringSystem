using Domain.Entities;
using Application.UseCases;
using Infrastructure.Dtos;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para la gesti�n de inventarios de tanques
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
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
        /// Obtiene el inventario m�s reciente por ID de estaci�n
        /// </summary>
        /// <param name="idEstacion">ID de la estaci�n</param>
        /// <returns>Los registros de inventario m�s recientes para la estaci�n especificada</returns>
        [HttpGet("{idEstacion:int}")]
        [SwaggerOperation(
            Summary = "Obtiene el inventario m�s reciente por estaci�n",
            Description = "Retorna los registros de inventario m�s recientes para una estaci�n espec�fica",
            OperationId = "GetInventarioByEstacion",
            Tags = new[] { "Inventarios" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<IEnumerable<InventarioEntity>>> GetByEstacion(int idEstacion)
        {
            if (idEstacion <= 0)
            {
                _logger.LogWarning("Invalid station ID provided: {IdEstacion}", idEstacion);
                return BadRequest(new { message = "El ID de estaci�n proporcionado no es v�lido" });
            }

            var result = await _getLatestInventarioByStationUseCase.ExecuteAsync(s => s.Idestacion == idEstacion);
            
            if (result == null || !result.Any())
            {
                _logger.LogInformation("No inventory records found for station {IdEstacion}", idEstacion);
                return NoContent();
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un registro de inventario por su ID
        /// </summary>
        /// <param name="id">ID del registro de inventario a consultar</param>
        /// <returns>El registro de inventario solicitado</returns>
        [HttpGet("id/{id:int}")]
        [SwaggerOperation(
            Summary = "Obtiene un inventario por su ID",
            Description = "Retorna informaci�n detallada de un registro de inventario espec�fico",
            OperationId = "GetInventarioById",
            Tags = new[] { "Inventarios" }
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<InventarioEntity>> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid inventory ID provided: {Id}", id);
                return BadRequest(new { message = "El ID proporcionado no es v�lido" });
            }

            var result = await _getInventarioByIdUseCase.ExecuteAsync(id);
            
            if (result == null)
            {
                _logger.LogInformation("Inventory with ID {Id} not found", id);
                return NotFound(new { message = $"No se encontr� un inventario con el ID {id}" });
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo registro de inventario
        /// </summary>
        /// <param name="inventarioRequest">Datos del inventario a crear</param>
        /// <returns>Respuesta indicando el �xito o fracaso de la operaci�n</returns>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Crea un nuevo registro de inventario",
            Description = "Registra un nuevo inventario en el sistema",
            OperationId = "CreateInventario",
            Tags = new[] { "Inventarios" }
        )]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> Create([FromBody] InventarioRequestDTO inventarioRequest)
        {
            if (inventarioRequest == null)
            {
                return BadRequest(new { message = "El cuerpo de la solicitud no puede estar vac�o" });
            }

            var validationErrors = ValidateInventarioRequest(inventarioRequest);
            if (validationErrors.Any())
            {
                return BadRequest(new { message = "Datos de inventario inv�lidos", errors = validationErrors });
            }

            await _createInventarioUseCase.ExecuteAsync(inventarioRequest);
            
            _logger.LogInformation(
                "Inventory created successfully for Estacion {IdEstacion} and Tanque {NoTanque}",
                inventarioRequest.IdEstacion, inventarioRequest.NoTanque);
            
            return Created($"/api/inventarios", new { 
                message = "Inventario creado con �xito", 
                idEstacion = inventarioRequest.IdEstacion,
                noTanque = inventarioRequest.NoTanque
            });
        }

        private static List<string> ValidateInventarioRequest(InventarioRequestDTO request)
        {
            var errors = new List<string>();
            
            if (request.IdEstacion <= 0)
                errors.Add("El ID de estaci�n es obligatorio y debe ser positivo");
                
            if (request.NoTanque <= 0)
                errors.Add("El n�mero de tanque es obligatorio y debe ser positivo");
                
            if (request.VolumenDisponible <= 0)
                errors.Add("El volumen disponible debe ser un valor positivo");
                
            return errors;
        }
    }
}