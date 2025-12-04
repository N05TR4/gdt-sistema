using GDT.Application.Interfaces;
using GDT.Domain.Entities;
using GDT.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GDT.Infrastructure.Repositories;

public class DeclaracionRepository : IDeclaracionRepository
{
    private readonly GdtDbContext _context;

    public DeclaracionRepository(GdtDbContext context)
    {
        _context = context;
    }

    public async Task<Declaracion?> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Declaraciones
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Declaracion?> ObtenerPorNumeroAsync(
        string numeroDeclaracion,
        CancellationToken cancellationToken = default)
    {
        return await _context.Declaraciones
            .FirstOrDefaultAsync(d => d.NumeroDeclaracion == numeroDeclaracion, cancellationToken);
    }

    public async Task<List<Declaracion>> ObtenerPorRNCAsync(
        string rnc,
        int pagina,
        int tamañoPagina,
        CancellationToken cancellationToken = default)
    {
        return await _context.Declaraciones
            .Where(d => d.RNC == rnc)
            .OrderByDescending(d => d.FechaCreacion)
            .Skip((pagina - 1) * tamañoPagina)
            .Take(tamañoPagina)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ContarPorRNCAsync(
        string rnc,
        CancellationToken cancellationToken = default)
    {
        return await _context.Declaraciones
            .CountAsync(d => d.RNC == rnc, cancellationToken);
    }

    public async Task<Declaracion> CrearAsync(
        Declaracion declaracion,
        CancellationToken cancellationToken = default)
    {
        _context.Declaraciones.Add(declaracion);
        await _context.SaveChangesAsync(cancellationToken);
        return declaracion;
    }

    public async Task ActualizarAsync(
        Declaracion declaracion,
        CancellationToken cancellationToken = default)
    {
        _context.Declaraciones.Update(declaracion);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExisteDeclaracionParaPeriodoAsync(
        string rnc,
        DateOnly periodo,
        TipoImpuesto tipoImpuesto,
        CancellationToken cancellationToken = default)
    {
        return await _context.Declaraciones
            .AnyAsync(d => d.RNC == rnc 
                        && d.Periodo == periodo 
                        && d.TipoImpuesto == tipoImpuesto
                        && d.Estado != EstadoDeclaracion.Rechazada,
                      cancellationToken);
    }
}
