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
    /// Controlador para la gestión de inventarios de tanques
    /// </summary>
    /// <remarks>
    /// Este controlador permite realizar operaciones CRUD sobre los registros de inventario de tanques.
    /// Los inventarios representan el estado actual de los tanques de combustible en las estaciones.
    /// </remarks>
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
        /// Obtiene el inventario más reciente por ID de estación
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna los registros de inventario más recientes para una estación específica.
        /// 
        /// La información incluye:
        /// - Nivel actual de cada tanque
        /// - Volumen disponible
        /// - Temperatura del producto
        /// - Fecha y hora de la última medición
        /// - Información sobre el producto almacenado
        /// 
        /// Si el ID de estación no es válido (menor o igual a cero), se retorna BadRequest.
        /// Si no existen registros de inventario para la estación, se retorna NoContent.
        /// </remarks>
        /// <param name="idEstacion">ID de la estación</param>
        /// <returns>Los registros de inventario más recientes para la estación especificada</returns>
        /// <response code="200">Retorna los registros de inventario</response>
        /// <response code="204">Si no existen registros de inventario para la estación</response>
        /// <response code="400">Si el ID de estación proporcionado no es válido</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para acceder a este recurso</response>
        /// <response code="500">Si ocurrió un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexión con la base de datos</response>
        [HttpGet("{idEstacion:int}")]
        [SwaggerOperation(
            Summary = "Obtiene el inventario más reciente por estación",
            Description = "Retorna los registros de inventario más recientes para una estación específica",
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
        /// Obtiene un registro de inventario por su ID
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna información detallada de un registro de inventario específico según su ID.
        /// 
        /// La información incluye:
        /// - Estación y tanque asociados
        /// - Volumen disponible
        /// - Temperatura del producto
        /// - Fecha y hora de la medición
        /// - Información sobre el producto almacenado
        /// 
        /// Si el ID no es válido (menor o igual a cero), se retorna BadRequest.
        /// Si no se encuentra un registro de inventario con el ID especificado, se retorna NotFound.
        /// </remarks>
        /// <param name="id">ID del registro de inventario a consultar</param>
        /// <returns>El registro de inventario solicitado</returns>
        /// <response code="200">Retorna el registro de inventario solicitado</response>
        /// <response code="400">Si el ID proporcionado no es válido</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para acceder a este recurso</response>
        /// <response code="404">Si no se encontró el registro de inventario</response>
        /// <response code="500">Si ocurrió un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexión con la base de datos</response>
        [HttpGet("id/{id:int}")]
        [SwaggerOperation(
            Summary = "Obtiene un inventario por su ID",
            Description = "Retorna información detallada de un registro de inventario específico",
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
        /// Crea un nuevo registro de inventario
        /// </summary>
        /// <remarks>
        /// Este endpoint permite registrar un nuevo inventario en el sistema.
        /// 
        /// **Campos requeridos:**
        /// - IdEstacion: ID de la estación (valor positivo)
        /// - NoTanque: Número del tanque (valor positivo)
        /// - VolumenDisponible: Volumen disponible (en litros, valor positivo)
        /// 
        /// **Campos opcionales:**
        /// - Temperatura: Temperatura del producto (en grados Celsius)
        /// - Fecha: Fecha y hora de la medición (por defecto, la fecha actual)
        /// - Observaciones: Notas adicionales sobre la medición
        /// 
        /// Ejemplo de solicitud:
        /// 
        /// ```json
        /// {
        ///   "idEstacion": 1,
        ///   "noTanque": 2,
        ///   "volumenDisponible": 3500.75,
        ///   "temperatura": 25.5,
        ///   "fecha": "2023-06-15T10:30:00Z",
        ///   "observaciones": "Medición rutinaria programada"
        /// }
        /// ```
        /// </remarks>
        /// <param name="inventarioRequest">Datos del inventario a crear</param>
        /// <returns>Respuesta indicando el éxito o fracaso de la operación</returns>
        /// <response code="201">Si el inventario se creó exitosamente</response>
        /// <response code="400">Si los datos de la solicitud son inválidos</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permisos para realizar esta acción</response>
        /// <response code="409">Si hay un conflicto con un registro existente</response>
        /// <response code="500">Si ocurrió un error interno al procesar la solicitud</response>
        /// <response code="503">Si hay un problema de conexión con la base de datos</response>
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