using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using TransporteApp.Data;
using TransporteApp.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace TransporteApp.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly TransporteDbContext _context;

    [ObservableProperty]
    private decimal gananciaDelMes;

    [ObservableProperty]
    private int viajesEnCurso;

    [ObservableProperty]
    private decimal litrosTransportadosHoy;

    [ObservableProperty]
    private ObservableCollection<Viaje> viajesRecientes = new();

    [ObservableProperty]
    private bool isLoading;

    public DashboardViewModel(TransporteDbContext context)
    {
        _context = context;
        LoadDashboardDataCommand = new AsyncRelayCommand(LoadDashboardDataAsync);
    }

    public IAsyncRelayCommand LoadDashboardDataCommand { get; }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            IsLoading = true;

            // Ejecutar secuencialmente para evitar problemas de concurrencia con DbContext
            // DbContext no es thread-safe, así que no podemos usar Task.WhenAll
            await LoadGananciaDelMesAsync();
            await LoadViajesEnCursoAsync();
            await LoadLitrosTransportadosHoyAsync();
            await LoadViajesRecientesAsync();
        }
        catch (Exception ex)
        {
            // En caso de error (ej: tablas no existen), establecer valores por defecto
            GananciaDelMes = 0;
            ViajesEnCurso = 0;
            LitrosTransportadosHoy = 0;
            ViajesRecientes.Clear();
            
            // Si es un error de tabla no encontrada, intentar crear las tablas
            if (ex.Message.Contains("no existe la relación") || 
                ex.Message.Contains("does not exist") ||
                ex.InnerException?.Message?.Contains("no existe la relación") == true ||
                ex.InnerException?.Message?.Contains("does not exist") == true)
            {
                try
                {
                    await _context.Database.EnsureCreatedAsync();
                    // Reintentar cargar los datos una vez
                    try
                    {
                        await LoadGananciaDelMesAsync();
                        await LoadViajesEnCursoAsync();
                        await LoadLitrosTransportadosHoyAsync();
                        await LoadViajesRecientesAsync();
                        return; // Éxito, salir sin lanzar excepción
                    }
                    catch
                    {
                        // Si aún falla después de crear las tablas, continuar con el error
                    }
                }
                catch (Exception createEx)
                {
                    // Si no se pueden crear las tablas, lanzar el error original
                    throw new Exception($"No se pudieron crear las tablas en la base de datos: {createEx.Message}", ex);
                }
            }
            
            // Re-lanzar la excepción para que MainWindow la maneje
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadGananciaDelMesAsync()
    {
        try
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var viajesCompletados = await _context.Viajes
                .Where(v => v.Estado == "Completado" &&
                           v.FechaSalida >= inicioMes &&
                           v.FechaSalida <= finMes)
                .Include(v => v.Gastos)
                .ToListAsync();

            GananciaDelMes = viajesCompletados.Sum(v => v.UtilidadNeta);
        }
        catch
        {
            GananciaDelMes = 0;
            throw; // Re-lanzar para que el catch principal lo maneje
        }
    }

    private async Task LoadViajesEnCursoAsync()
    {
        try
        {
            ViajesEnCurso = await _context.Viajes
                .CountAsync(v => v.Estado == "EnCurso");
        }
        catch
        {
            ViajesEnCurso = 0;
            throw;
        }
    }

    private async Task LoadLitrosTransportadosHoyAsync()
    {
        try
        {
            var hoy = DateTime.Now.Date;
            var inicioDia = hoy;
            var finDia = hoy.AddDays(1).AddTicks(-1);

            LitrosTransportadosHoy = await _context.Viajes
                .Where(v => v.FechaSalida >= inicioDia && 
                           v.FechaSalida < finDia && 
                           v.Estado != "Cancelado")
                .SumAsync(v => (decimal?)v.CantidadLitros) ?? 0m;
        }
        catch
        {
            LitrosTransportadosHoy = 0;
            throw;
        }
    }

    private async Task LoadViajesRecientesAsync()
    {
        try
        {
            var viajes = await _context.Viajes
                .Include(v => v.Vehiculo)
                .Include(v => v.Conductor)
                .Include(v => v.Cliente)
                .OrderByDescending(v => v.FechaSalida)
                .Take(5)
                .ToListAsync();

            ViajesRecientes.Clear();
            foreach (var viaje in viajes)
            {
                ViajesRecientes.Add(viaje);
            }
        }
        catch
        {
            ViajesRecientes.Clear();
            throw;
        }
    }
}

