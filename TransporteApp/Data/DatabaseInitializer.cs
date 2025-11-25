using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TransporteApp.Data;

public class DatabaseInitializer
{
    private readonly TransporteDbContext _context;
    private readonly ILogger<DatabaseInitializer>? _logger;

    public DatabaseInitializer(TransporteDbContext context, ILogger<DatabaseInitializer>? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Verificar si la base de datos existe, si no, crearla
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (!canConnect)
            {
                _logger?.LogInformation("La base de datos no existe. Creando...");
                await _context.Database.EnsureCreatedAsync();
                _logger?.LogInformation("Base de datos creada exitosamente.");
            }
            else
            {
                // Aplicar migraciones pendientes si existen
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger?.LogInformation("Aplicando migraciones pendientes...");
                    await _context.Database.MigrateAsync();
                    _logger?.LogInformation("Migraciones aplicadas exitosamente.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al inicializar la base de datos.");
            throw;
        }
    }
}

