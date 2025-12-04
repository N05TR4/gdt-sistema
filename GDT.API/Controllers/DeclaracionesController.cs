using GDT.Application.DTOs;
using GDT.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GDT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeclaracionesController : ControllerBase
{
    private readonly DeclaracionesService _service;
    private readonly ILogger<DeclaracionesController> _logger;

    public DeclaracionesController(
        DeclaracionesService service,
        ILogger<DeclaracionesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Crea una nueva declaración tributaria en estado borrador
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DeclaracionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DeclaracionDto>> CrearDeclaracion(
        [FromBody] CrearDeclaracionDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var declaracion = await _service.CrearDeclaracionAsync(dto, cancellationToken);
            return CreatedAtAction(
                nameof(ObtenerDeclaracion),
                new { id = declaracion.Id },
                declaracion);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Datos inválidos al crear declaración");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operación inválida al crear declaración");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene los detalles de una declaración específica
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DeclaracionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeclaracionDto>> ObtenerDeclaracion(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var declaracion = await _service.ObtenerDeclaracionAsync(id, cancellationToken);
            return Ok(declaracion);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Declaración {id} no encontrada" });
        }
    }

    /// <summary>
    /// Lista todas las declaraciones de un contribuyente
    /// </summary>
    [HttpGet("contribuyente/{rnc}")]
    [ProducesResponseType(typeof(PaginacionDto<DeclaracionResumenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginacionDto<DeclaracionResumenDto>>> ListarDeclaracionesPorRNC(
        string rnc,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamañoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        if (pagina < 1) pagina = 1;
        if (tamañoPagina < 1 || tamañoPagina > 100) tamañoPagina = 10;

        var resultado = await _service.ObtenerDeclaracionesPorRNCAsync(
            rnc,
            pagina,
            tamañoPagina,
            cancellationToken);

        return Ok(resultado);
    }

    /// <summary>
    /// Actualiza los montos de una declaración en estado borrador
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DeclaracionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeclaracionDto>> ActualizarDeclaracion(
        Guid id,
        [FromBody] ActualizarDeclaracionDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var declaracion = await _service.ActualizarDeclaracionAsync(id, dto, cancellationToken);
            return Ok(declaracion);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Declaración {id} no encontrada" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Presenta oficialmente una declaración (cambia estado a Presentada)
    /// </summary>
    [HttpPost("{id:guid}/presentar")]
    [ProducesResponseType(typeof(DeclaracionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeclaracionDto>> PresentarDeclaracion(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var declaracion = await _service.PresentarDeclaracionAsync(id, cancellationToken);
            return Ok(declaracion);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Declaración {id} no encontrada" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Health check del servicio
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new 
        { 
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            service = "Declaraciones API"
        });
    }
}
