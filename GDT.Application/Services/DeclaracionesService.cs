using GDT.Application.DTOs;
using GDT.Application.Interfaces;
using GDT.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GDT.Application.Services;

public class DeclaracionesService
{
    private readonly IDeclaracionRepository _repository;
    private readonly ILogger<DeclaracionesService> _logger;

    public DeclaracionesService(
        IDeclaracionRepository repository,
        ILogger<DeclaracionesService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DeclaracionDto> CrearDeclaracionAsync(
        CrearDeclaracionDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creando declaración para RNC: {RNC}, Periodo: {Periodo}", 
            dto.RNC, dto.Periodo);

        // Validar que no exista declaración para el mismo periodo
        var existe = await _repository.ExisteDeclaracionParaPeriodoAsync(
            dto.RNC,
            dto.Periodo,
            (TipoImpuesto)dto.TipoImpuesto,
            cancellationToken);

        if (existe)
        {
            throw new InvalidOperationException(
                $"Ya existe una declaración para el RNC {dto.RNC} en el periodo {dto.Periodo}");
        }

        var declaracion = Declaracion.Crear(
            dto.RNC,
            dto.RazonSocial,
            dto.Periodo,
            (TipoImpuesto)dto.TipoImpuesto,
            dto.MontoIngresos,
            dto.MontoGastos);

        await _repository.CrearAsync(declaracion, cancellationToken);

        _logger.LogInformation("Declaración creada exitosamente: {NumeroDeclaracion}", 
            declaracion.NumeroDeclaracion);

        return MapearADto(declaracion);
    }

    public async Task<DeclaracionDto> ObtenerDeclaracionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var declaracion = await _repository.ObtenerPorIdAsync(id, cancellationToken);
        
        if (declaracion == null)
        {
            throw new KeyNotFoundException($"Declaración {id} no encontrada");
        }

        return MapearADto(declaracion);
    }

    public async Task<PaginacionDto<DeclaracionResumenDto>> ObtenerDeclaracionesPorRNCAsync(
        string rnc,
        int pagina,
        int tamañoPagina,
        CancellationToken cancellationToken = default)
    {
        var declaraciones = await _repository.ObtenerPorRNCAsync(rnc, pagina, tamañoPagina, cancellationToken);
        var total = await _repository.ContarPorRNCAsync(rnc, cancellationToken);

        return new PaginacionDto<DeclaracionResumenDto>
        {
            Items = declaraciones.Select(MapearAResumenDto).ToList(),
            TotalItems = total,
            PaginaActual = pagina,
            TamañoPagina = tamañoPagina
        };
    }

    public async Task<DeclaracionDto> ActualizarDeclaracionAsync(
        Guid id,
        ActualizarDeclaracionDto dto,
        CancellationToken cancellationToken = default)
    {
        var declaracion = await _repository.ObtenerPorIdAsync(id, cancellationToken);
        
        if (declaracion == null)
        {
            throw new KeyNotFoundException($"Declaración {id} no encontrada");
        }

        _logger.LogInformation("Actualizando declaración: {NumeroDeclaracion}", 
            declaracion.NumeroDeclaracion);

        declaracion.ActualizarMontos(dto.MontoIngresos, dto.MontoGastos);
        await _repository.ActualizarAsync(declaracion, cancellationToken);

        _logger.LogInformation("Declaración actualizada exitosamente: {NumeroDeclaracion}", 
            declaracion.NumeroDeclaracion);

        return MapearADto(declaracion);
    }

    public async Task<DeclaracionDto> PresentarDeclaracionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var declaracion = await _repository.ObtenerPorIdAsync(id, cancellationToken);
        
        if (declaracion == null)
        {
            throw new KeyNotFoundException($"Declaración {id} no encontrada");
        }

        _logger.LogInformation("Presentando declaración: {NumeroDeclaracion}", 
            declaracion.NumeroDeclaracion);

        declaracion.Presentar();
        await _repository.ActualizarAsync(declaracion, cancellationToken);

        _logger.LogInformation("Declaración presentada exitosamente: {NumeroDeclaracion}. Total a pagar: {Total}", 
            declaracion.NumeroDeclaracion, declaracion.TotalAPagar);

        return MapearADto(declaracion);
    }

    private static DeclaracionDto MapearADto(Declaracion declaracion)
    {
        return new DeclaracionDto
        {
            Id = declaracion.Id,
            NumeroDeclaracion = declaracion.NumeroDeclaracion,
            RNC = declaracion.RNC,
            RazonSocial = declaracion.RazonSocial,
            Periodo = declaracion.Periodo,
            TipoImpuesto = declaracion.TipoImpuesto.ToString(),
            MontoIngresos = declaracion.MontoIngresos,
            MontoGastos = declaracion.MontoGastos,
            BaseImponible = declaracion.BaseImponible,
            ImpuestoCalculado = declaracion.ImpuestoCalculado,
            Sancion = declaracion.Sancion,
            TotalAPagar = declaracion.TotalAPagar,
            Estado = declaracion.Estado.ToString(),
            FechaCreacion = declaracion.FechaCreacion,
            FechaPresentacion = declaracion.FechaPresentacion,
            ObservacionesRechazo = declaracion.ObservacionesRechazo
        };
    }

    private static DeclaracionResumenDto MapearAResumenDto(Declaracion declaracion)
    {
        return new DeclaracionResumenDto
        {
            Id = declaracion.Id,
            NumeroDeclaracion = declaracion.NumeroDeclaracion,
            Periodo = declaracion.Periodo,
            TipoImpuesto = declaracion.TipoImpuesto.ToString(),
            TotalAPagar = declaracion.TotalAPagar,
            Estado = declaracion.Estado.ToString(),
            FechaCreacion = declaracion.FechaCreacion
        };
    }
}
