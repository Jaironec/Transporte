using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using TransporteApp.Data;
using TransporteApp.Models;

namespace TransporteApp.ViewModels;

public partial class VehiculoViewModel : BaseViewModel
{
    private readonly TransporteDbContext _context;

    [ObservableProperty]
    private ObservableCollection<Vehiculo> vehiculos = new();

    [ObservableProperty]
    private Vehiculo? vehiculoSeleccionado;

    [ObservableProperty]
    private bool isEditing;

    public VehiculoViewModel(TransporteDbContext context)
    {
        _context = context;
        LoadVehiculosCommand = new AsyncRelayCommand(LoadVehiculosAsync);
        GuardarCommand = new AsyncRelayCommand(GuardarAsync);
        EliminarCommand = new AsyncRelayCommand(EliminarAsync);
        NuevoCommand = new RelayCommand(Nuevo);
        CancelarCommand = new RelayCommand(Cancelar);
    }

    public IAsyncRelayCommand LoadVehiculosCommand { get; }
    public IAsyncRelayCommand GuardarCommand { get; }
    public IAsyncRelayCommand EliminarCommand { get; }
    public IRelayCommand NuevoCommand { get; }
    public IRelayCommand CancelarCommand { get; }

    private async Task LoadVehiculosAsync()
    {
        try
        {
            var items = await _context.Vehiculos.ToListAsync();
            Vehiculos.Clear();
            foreach (var item in items)
            {
                Vehiculos.Add(item);
            }
        }
        catch (Exception ex)
        {
            MensajeError = $"Error al cargar vehículos: {ex.Message}";
        }
    }

    private void Nuevo()
    {
        VehiculoSeleccionado = new Vehiculo 
        { 
            Placa = string.Empty, 
            Marca = string.Empty, 
            CapacidadLitros = 0, 
            Estado = "Activo" 
        };
        IsEditing = true;
    }

    private void Cancelar()
    {
        VehiculoSeleccionado = null;
        IsEditing = false;
        MensajeError = string.Empty;
    }

    private async Task GuardarAsync()
    {
        if (VehiculoSeleccionado == null) return;

        try
        {
            if (VehiculoSeleccionado.Id == 0)
            {
                _context.Vehiculos.Add(VehiculoSeleccionado);
            }
            else
            {
                _context.Vehiculos.Update(VehiculoSeleccionado);
            }

            await _context.SaveChangesAsync();
            await LoadVehiculosAsync();
            IsEditing = false;
            VehiculoSeleccionado = null;
        }
        catch (Exception ex)
        {
            MensajeError = $"Error al guardar: {ex.Message}";
        }
    }

    private async Task EliminarAsync()
    {
        if (VehiculoSeleccionado == null || VehiculoSeleccionado.Id == 0) return;

        try
        {
            _context.Vehiculos.Remove(VehiculoSeleccionado);
            await _context.SaveChangesAsync();
            await LoadVehiculosAsync();
            IsEditing = false;
            VehiculoSeleccionado = null;
        }
        catch (Exception ex)
        {
            MensajeError = $"Error al eliminar: {ex.Message}";
        }
    }
}
