using GDT.Application.Interfaces;
using GDT.Application.Services;
using GDT.Infrastructure.Data;
using GDT.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllers();

// Configurar base de datos (SQL Server)
builder.Services.AddDbContext<GdtDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=(localdb)\\mssqllocaldb;Database=GdtDb;Trusted_Connection=True;MultipleActiveResultSets=true";
    
    options.UseSqlServer(connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null));
});

// Dependency Injection
builder.Services.AddScoped<IDeclaracionRepository, DeclaracionRepository>();
builder.Services.AddScoped<DeclaracionesService>();

// Configuración de Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GDT - Sistema de Gestión de Declaraciones Tributarias",
        Version = "v1",
        Description = "API para gestión de declaraciones tributarias de DGII República Dominicana",
        Contact = new OpenApiContact
        {
            Name = "DGII - Dirección General de Impuestos Internos",
            Email = "sistemas@dgii.gov.do"
        }
    });

    // Incluir comentarios XML para documentación
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// CORS (para desarrollo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Logging con Serilog (opcional)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<GdtDbContext>();

var app = builder.Build();

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GDT API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
    });

    // Crear base de datos automáticamente en desarrollo
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<GdtDbContext>();
        dbContext.Database.EnsureCreated();
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Logger.LogInformation("Iniciando GDT API - Sistema de Gestión de Declaraciones Tributarias");
app.Logger.LogInformation("Ambiente: {Environment}", app.Environment.EnvironmentName);

app.Run();
