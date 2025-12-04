namespace GDT.Domain.Entities;

public class Declaracion
{
    public Guid Id { get; private set; }
    public string NumeroDeclaracion { get; private set; }
    public string RNC { get; private set; }
    public string RazonSocial { get; private set; }
    public DateOnly Periodo { get; private set; }
    public TipoImpuesto TipoImpuesto { get; private set; }
    public decimal MontoIngresos { get; private set; }
    public decimal MontoGastos { get; private set; }
    public decimal BaseImponible => MontoIngresos - MontoGastos;
    public decimal ImpuestoCalculado { get; private set; }
    public decimal Sancion { get; private set; }
    public decimal TotalAPagar => ImpuestoCalculado + Sancion;
    public EstadoDeclaracion Estado { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaPresentacion { get; private set; }
    public string? ObservacionesRechazo { get; private set; }

    private Declaracion() { } // Para EF Core

    public static Declaracion Crear(
        string rnc,
        string razonSocial,
        DateOnly periodo,
        TipoImpuesto tipoImpuesto,
        decimal montoIngresos,
        decimal montoGastos)
    {
        ValidarDatos(rnc, razonSocial, montoIngresos, montoGastos);

        var declaracion = new Declaracion
        {
            Id = Guid.NewGuid(),
            NumeroDeclaracion = GenerarNumeroDeclaracion(),
            RNC = rnc,
            RazonSocial = razonSocial,
            Periodo = periodo,
            TipoImpuesto = tipoImpuesto,
            MontoIngresos = montoIngresos,
            MontoGastos = montoGastos,
            Estado = EstadoDeclaracion.Borrador,
            FechaCreacion = DateTime.UtcNow
        };

        declaracion.CalcularImpuesto();
        return declaracion;
    }

    public void ActualizarMontos(decimal montoIngresos, decimal montoGastos)
    {
        if (Estado != EstadoDeclaracion.Borrador)
            throw new InvalidOperationException("No se puede modificar una declaración que no está en estado Borrador");

        if (montoIngresos < 0 || montoGastos < 0)
            throw new ArgumentException("Los montos no pueden ser negativos");

        MontoIngresos = montoIngresos;
        MontoGastos = montoGastos;
        CalcularImpuesto();
    }

    public void Presentar()
    {
        if (Estado != EstadoDeclaracion.Borrador)
            throw new InvalidOperationException("Solo se pueden presentar declaraciones en estado Borrador");

        ValidarParaPresentacion();

        Estado = EstadoDeclaracion.Presentada;
        FechaPresentacion = DateTime.UtcNow;

        // Calcular sanción si es presentación tardía
        var fechaLimite = ObtenerFechaLimite(Periodo, TipoImpuesto);
        if (FechaPresentacion > fechaLimite)
        {
            CalcularSancion(fechaLimite);
        }
    }

    public void Aprobar()
    {
        if (Estado != EstadoDeclaracion.Presentada)
            throw new InvalidOperationException("Solo se pueden aprobar declaraciones Presentadas");

        Estado = EstadoDeclaracion.Aprobada;
    }

    public void Rechazar(string observaciones)
    {
        if (Estado != EstadoDeclaracion.Presentada)
            throw new InvalidOperationException("Solo se pueden rechazar declaraciones Presentadas");

        Estado = EstadoDeclaracion.Rechazada;
        ObservacionesRechazo = observaciones;
    }

    private void CalcularImpuesto()
    {
        if (BaseImponible <= 0)
        {
            ImpuestoCalculado = 0;
            return;
        }

        ImpuestoCalculado = TipoImpuesto switch
        {
            TipoImpuesto.ISR => CalcularISR(BaseImponible),
            TipoImpuesto.ITBIS => CalcularITBIS(BaseImponible),
            TipoImpuesto.Selectivo => CalcularSelectivo(BaseImponible),
            _ => throw new NotImplementedException($"Cálculo no implementado para {TipoImpuesto}")
        };
    }

    private static decimal CalcularISR(decimal baseImponible)
    {
        // Tabla de ISR simplificada para RD
        return baseImponible switch
        {
            <= 416220 => 0,
            <= 624329 => (baseImponible - 416220) * 0.15m,
            <= 867123 => 31216 + (baseImponible - 624329) * 0.20m,
            _ => 79775 + (baseImponible - 867123) * 0.25m
        };
    }

    private static decimal CalcularITBIS(decimal baseImponible)
    {
        // ITBIS 18%
        return baseImponible * 0.18m;
    }

    private static decimal CalcularSelectivo(decimal baseImponible)
    {
        // Impuesto selectivo variable (ejemplo 10%)
        return baseImponible * 0.10m;
    }

    private void CalcularSancion(DateTime fechaLimite)
    {
        var diasRetraso = (FechaPresentacion!.Value - fechaLimite).Days;
        
        // 10% del impuesto + 4% mensual
        var sancionBase = ImpuestoCalculado * 0.10m;
        var mesesRetraso = (int)Math.Ceiling(diasRetraso / 30.0);
        var sancionPorMora = ImpuestoCalculado * 0.04m * mesesRetraso;
        
        Sancion = sancionBase + sancionPorMora;
    }

    private static DateTime ObtenerFechaLimite(DateOnly periodo, TipoImpuesto tipoImpuesto)
    {
        // Simplificación: día 20 del mes siguiente al periodo
        var mesLimite = periodo.AddMonths(1);
        return new DateTime(mesLimite.Year, mesLimite.Month, 20, 23, 59, 59);
    }

    private void ValidarParaPresentacion()
    {
        if (string.IsNullOrWhiteSpace(RNC))
            throw new InvalidOperationException("RNC es requerido");

        if (MontoIngresos <= 0)
            throw new InvalidOperationException("Debe tener ingresos declarados");

        if (BaseImponible < 0)
            throw new InvalidOperationException("La base imponible no puede ser negativa");
    }

    private static void ValidarDatos(string rnc, string razonSocial, decimal montoIngresos, decimal montoGastos)
    {
        if (string.IsNullOrWhiteSpace(rnc))
            throw new ArgumentException("RNC es requerido");

        if (string.IsNullOrWhiteSpace(razonSocial))
            throw new ArgumentException("Razón social es requerida");

        if (montoIngresos < 0)
            throw new ArgumentException("Monto de ingresos no puede ser negativo");

        if (montoGastos < 0)
            throw new ArgumentException("Monto de gastos no puede ser negativo");

        if (!ValidarFormatoRNC(rnc))
            throw new ArgumentException("Formato de RNC inválido");
    }

    private static bool ValidarFormatoRNC(string rnc)
    {
        // RNC en RD: 9 dígitos
        return rnc.Length == 9 && rnc.All(char.IsDigit);
    }

    private static string GenerarNumeroDeclaracion()
    {
        var año = DateTime.UtcNow.Year;
        var secuencia = DateTime.UtcNow.Ticks % 1000000;
        return $"DECL-{año}-{secuencia:D6}";
    }
}

public enum TipoImpuesto
{
    ISR = 1,      // Impuesto Sobre la Renta
    ITBIS = 2,    // Impuesto a las Transferencias de Bienes Industrializados y Servicios
    Selectivo = 3 // Impuestos Selectivos
}

public enum EstadoDeclaracion
{
    Borrador = 1,
    Presentada = 2,
    Aprobada = 3,
    Rechazada = 4
}
