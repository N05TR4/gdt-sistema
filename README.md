# Sistema de Gestión de Declaraciones Tributarias (GDT)

Sistema de gestión de declaraciones tributarias para la Dirección General de Impuestos Internos (DGII) de República Dominicana.

## Descripción

Este sistema implementa una arquitectura en capas bien diseñada utilizando ASP.NET Core 8.0, demostrando las ventajas de un monolito modular para aplicaciones gubernamentales que requieren transacciones ACID, baja latencia y simplicidad operacional.

## Arquitectura

### Estructura del Proyecto

```
GDT.API/                    # Capa de presentación (Web API)
├── Controllers/            # Controladores REST
├── Program.cs             # Configuración y punto de entrada
└── appsettings.json       # Configuración

GDT.Application/           # Capa de aplicación
├── DTOs/                  # Data Transfer Objects
├── Services/              # Servicios de aplicación
└── Interfaces/            # Contratos de repositorios

GDT.Domain/                # Capa de dominio
└── Entities/              # Entidades de dominio con lógica de negocio

GDT.Infrastructure/        # Capa de infraestructura
├── Data/                  # DbContext de Entity Framework
└── Repositories/          # Implementaciones de repositorios
```

### Principios Arquitectónicos

- **Separación de Responsabilidades**: Cada capa tiene un propósito bien definido
- **Dependencia Invertida**: Las capas externas dependen de abstracciones del dominio
- **Domain-Driven Design**: Entidades ricas con lógica de negocio encapsulada
- **Clean Architecture**: El dominio no tiene dependencias externas

## Tecnologías Utilizadas

- **.NET 8.0**: Framework principal
- **ASP.NET Core 8.0**: Web API
- **Entity Framework Core 8.0**: ORM
- **SQL Server**: Base de datos
- **Swagger/OpenAPI**: Documentación de API

## Requisitos Previos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2022 Express (o SQL Server LocalDB incluido con Visual Studio)
- Visual Studio 2022 o Visual Studio Code (opcional)
- Postman (para probar la API)

## Instalación y Ejecución

### 1. Clonar el Repositorio

```bash
git clone https://github.com/[usuario]/gdt-sistema.git
cd gdt-sistema
```

### 2. Restaurar Dependencias

```bash
cd GDT.API
dotnet restore
```

### 3. Configurar Base de Datos

Editar `appsettings.json` si es necesario para ajustar la cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GdtDb;Trusted_Connection=True;"
  }
}
```

La base de datos se crea automáticamente al ejecutar la aplicación en modo desarrollo.

### 4. Ejecutar la Aplicación

```bash
dotnet run
```

La API estará disponible en:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001` (en desarrollo)

## Endpoints de la API

### Crear Declaración
```http
POST /api/declaraciones
Content-Type: application/json

{
  "rnc": "123456789",
  "razonSocial": "Empresa Ejemplo SRL",
  "periodo": "2024-11-01",
  "tipoImpuesto": 1,
  "montoIngresos": 1000000.00,
  "montoGastos": 600000.00
}
```

### Obtener Declaración
```http
GET /api/declaraciones/{id}
```

### Listar Declaraciones por RNC
```http
GET /api/declaraciones/contribuyente/{rnc}?pagina=1&tamañoPagina=10
```

### Actualizar Declaración
```http
PUT /api/declaraciones/{id}
Content-Type: application/json

{
  "montoIngresos": 1200000.00,
  "montoGastos": 700000.00
}
```

### Presentar Declaración
```http
POST /api/declaraciones/{id}/presentar
```

### Health Check
```http
GET /api/declaraciones/health
```

## Tipos de Impuesto

1. **ISR** - Impuesto Sobre la Renta
2. **ITBIS** - Impuesto a las Transferencias de Bienes Industrializados y Servicios (18%)
3. **Selectivo** - Impuestos Selectivos

## Estados de Declaración

1. **Borrador** - Declaración en proceso de creación (puede modificarse)
2. **Presentada** - Declaración oficialmente presentada (no puede modificarse)
3. **Aprobada** - Declaración aprobada por DGII
4. **Rechazada** - Declaración rechazada por DGII

## Reglas de Negocio

### Cálculo de ISR (Simplificado)

- Hasta RD$ 416,220: Exento
- RD$ 416,221 - RD$ 624,329: 15% sobre el excedente
- RD$ 624,330 - RD$ 867,123: RD$ 31,216 + 20% sobre el excedente
- Mayor a RD$ 867,123: RD$ 79,775 + 25% sobre el excedente

### Sanciones por Presentación Tardía

- **Sanción Base**: 10% del impuesto calculado
- **Mora Mensual**: 4% del impuesto por mes de retraso
- **Fecha Límite**: Día 20 del mes siguiente al periodo fiscal

## Colección de Postman

Una colección completa de Postman está incluida en el repositorio:
`/postman/GDT-API-Collection.json`

### Importar en Postman

1. Abrir Postman
2. Click en "Import"
3. Seleccionar el archivo `GDT-API-Collection.json`
4. Configurar variables de entorno:
   - `baseUrl`: `https://localhost:5001`
   - `rnc`: `123456789` (para pruebas)

## Testing

Ejecutar tests unitarios (cuando estén implementados):

```bash
cd GDT.Tests
dotnet test
```

## Deployment

### Publicar para Producción

```bash
dotnet publish -c Release -o ./publish
```

### Docker (Opcional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "GDT.API.dll"]
```

## Consideraciones de Seguridad

Para producción, implementar:

- **Autenticación JWT**: Tokens para usuarios autenticados
- **Autorización**: Roles y permisos granulares
- **HTTPS**: Solo tráfico encriptado
- **Rate Limiting**: Prevenir abuso de la API
- **Input Validation**: Validación estricta de todos los inputs
- **Auditoría**: Log completo de todas las operaciones sensibles

## Licencia

Propiedad de DGII República Dominicana. Todos los derechos reservados.

## Contacto

- **Organización**: DGII - Dirección General de Impuestos Internos
- **Email**: sistemas@dgii.gov.do
- **Website**: https://www.dgii.gov.do

## Notas Importantes

Este es un proyecto académico demostrativo. Para implementación en producción:

1. Implementar autenticación y autorización robusta
2. Agregar validaciones adicionales de RNC contra sistema real de JCE
3. Implementar integración con sistema de pagos
4. Agregar auditoría completa de todas las operaciones
5. Configurar backup y disaster recovery
6. Implementar monitoreo y alerting comprehensivo
7. Realizar pruebas de carga y optimización de performance

---

**Autor**: [Tu Nombre]  
**Universidad**: O&M  
**Curso**: Ingeniería de Software  
**Fecha**: Diciembre 2025
