namespace GDT.Application.DTOs;

public record CrearDeclaracionDto(
    string RNC,
    string RazonSocial,
    DateOnly Periodo,
    int TipoImpuesto,
    decimal MontoIngresos,
    decimal MontoGastos
);

public record ActualizarDeclaracionDto(
    decimal MontoIngresos,
    decimal MontoGastos
);

public record DeclaracionDto
{
    public Guid Id { get; init; }
    public string NumeroDeclaracion { get; init; } = string.Empty;
    public string RNC { get; init; } = string.Empty;
    public string RazonSocial { get; init; } = string.Empty;
    public DateOnly Periodo { get; init; }
    public string TipoImpuesto { get; init; } = string.Empty;
    public decimal MontoIngresos { get; init; }
    public decimal MontoGastos { get; init; }
    public decimal BaseImponible { get; init; }
    public decimal ImpuestoCalculado { get; init; }
    public decimal Sancion { get; init; }
    public decimal TotalAPagar { get; init; }
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaPresentacion { get; init; }
    public string? ObservacionesRechazo { get; init; }
}

public record DeclaracionResumenDto
{
    public Guid Id { get; init; }
    public string NumeroDeclaracion { get; init; } = string.Empty;
    public DateOnly Periodo { get; init; }
    public string TipoImpuesto { get; init; } = string.Empty;
    public decimal TotalAPagar { get; init; }
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
}

public record PaginacionDto<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalItems { get; init; }
    public int PaginaActual { get; init; }
    public int TamañoPagina { get; init; }
    public int TotalPaginas => (int)Math.Ceiling(TotalItems / (double)TamañoPagina);
}
