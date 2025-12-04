using GDT.Domain.Entities;

namespace GDT.Application.Interfaces;

public interface IDeclaracionRepository
{
    Task<Declaracion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Declaracion?> ObtenerPorNumeroAsync(string numeroDeclaracion, CancellationToken cancellationToken = default);
    Task<List<Declaracion>> ObtenerPorRNCAsync(string rnc, int pagina, int tamañoPagina, CancellationToken cancellationToken = default);
    Task<int> ContarPorRNCAsync(string rnc, CancellationToken cancellationToken = default);
    Task<Declaracion> CrearAsync(Declaracion declaracion, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Declaracion declaracion, CancellationToken cancellationToken = default);
    Task<bool> ExisteDeclaracionParaPeriodoAsync(string rnc, DateOnly periodo, TipoImpuesto tipoImpuesto, CancellationToken cancellationToken = default);
}
