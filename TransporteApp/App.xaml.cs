using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.IO;
using System.Windows;
using TransporteApp.Data;
using TransporteApp.ViewModels;
using TransporteApp.Views;

namespace TransporteApp;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private IServiceScope? _mainScope;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Manejar excepciones no controladas
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        DispatcherUnhandledException += App_DispatcherUnhandledException;

        try
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Inicializar la base de datos antes de crear la ventana
            InitializeDatabase();

            // Crear un scope para los servicios Scoped (ViewModels y DbContext)
            // Mantener el scope vivo mientras la aplicación esté ejecutándose
            _mainScope = _serviceProvider.CreateScope();
            var mainWindow = _mainScope.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            ShowErrorDialog(
                "Error al iniciar la aplicación",
                ex,
                "Verifique que:\n" +
                "1. PostgreSQL esté ejecutándose\n" +
                "2. La base de datos exista (ejecute Database/Script_PostgreSQL.sql)\n" +
                "3. Las credenciales en appsettings.json sean correctas");
            Shutdown();
        }
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            ShowErrorDialog("Error no controlado", ex);
        }
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        ShowErrorDialog("Error en la interfaz", e.Exception);
        e.Handled = true; // Marcar como manejado para evitar que la aplicación se cierre
    }

    private void ShowErrorDialog(string title, Exception ex, string? additionalInfo = null)
    {
        var message = $"{title}:\n\n{ex.Message}";
        if (ex.InnerException != null)
        {
            message += $"\n\nDetalle: {ex.InnerException.Message}";
        }
        if (!string.IsNullOrEmpty(additionalInfo))
        {
            message += $"\n\n{additionalInfo}";
        }
        message += $"\n\nStack Trace:\n{ex.StackTrace}";

        MessageBox.Show(
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Configuración - Buscar appsettings.json en múltiples ubicaciones
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var appSettingsFile = Path.Combine(basePath, "appsettings.json");
        
        // Si no está en el directorio del ejecutable, buscar en el directorio del proyecto
        if (!File.Exists(appSettingsFile))
        {
            // Intentar en el directorio del proyecto (TransporteApp)
            var projectPath = Path.Combine(basePath, "..", "..", "..", "appsettings.json");
            if (File.Exists(projectPath))
            {
                basePath = Path.GetDirectoryName(projectPath) ?? basePath;
            }
            else
            {
                // Intentar en el directorio actual de trabajo
                var currentDir = Directory.GetCurrentDirectory();
                var currentDirPath = Path.Combine(currentDir, "TransporteApp", "appsettings.json");
                if (File.Exists(currentDirPath))
                {
                    basePath = Path.Combine(currentDir, "TransporteApp");
                }
                else
                {
                    throw new FileNotFoundException(
                        $"No se encontró appsettings.json. Buscado en:\n" +
                        $"1. {appSettingsFile}\n" +
                        $"2. {projectPath}\n" +
                        $"3. {currentDirPath}",
                        "appsettings.json");
                }
            }
        }
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Base de datos
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("La cadena de conexión no está configurada en appsettings.json");
        }

        // Configurar Npgsql para manejar DateTime correctamente
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        
        // Configurar PostgreSQL - Usar Scoped para evitar problemas de concurrencia
        services.AddDbContext<TransporteDbContext>(options =>
            options.UseNpgsql(connectionString),
            ServiceLifetime.Scoped);
        
        // Agregar DbContextFactory para crear instancias cuando sea necesario
        services.AddDbContextFactory<TransporteDbContext>(options =>
            options.UseNpgsql(connectionString));

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddScoped<DashboardViewModel>();
        services.AddScoped<ViajeViewModel>();
        services.AddScoped<VehiculoViewModel>();
        services.AddScoped<ConductorViewModel>();
        services.AddScoped<ClienteViewModel>();

        // Views (Optional if using DataTemplates, but good for consistency)
        services.AddTransient<MainWindow>();
    }

    private void InitializeDatabase()
    {
        try
        {
            using var scope = _serviceProvider!.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TransporteDbContext>();
            
            // Verificar si la base de datos existe y crear las tablas si no existen
            var canConnect = context.Database.CanConnect();
            
            if (!canConnect)
            {
                MessageBox.Show(
                    "No se puede conectar a PostgreSQL.\n\n" +
                    "Verifique que:\n" +
                    "1. PostgreSQL esté ejecutándose\n" +
                    "2. La base de datos 'TransporteDB' exista\n" +
                    "3. Las credenciales en appsettings.json sean correctas\n\n" +
                    "Para crear la base de datos, ejecute en PostgreSQL:\n" +
                    "CREATE DATABASE TransporteDB;",
                    "Error de Conexión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // Crear las tablas si no existen (esto creará las tablas según el modelo de EF Core)
            context.Database.EnsureCreated();
        }
        catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "42P01")
        {
            // Tabla no existe - intentar crearla de nuevo
            try
            {
                using var scope = _serviceProvider!.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TransporteDbContext>();
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al crear las tablas:\n\n{ex.Message}\n\n" +
                    "Intente ejecutar el script SQL manualmente:\n" +
                    "Database/Script_PostgreSQL.sql",
                    "Error de Base de Datos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al inicializar la base de datos:\n\n{ex.Message}\n\n" +
                "La aplicación intentará continuar, pero algunas funciones pueden no estar disponibles.\n\n" +
                "Si el problema persiste, ejecute el script SQL manualmente:\n" +
                "Database/Script_PostgreSQL.sql",
                "Advertencia",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mainScope?.Dispose();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
