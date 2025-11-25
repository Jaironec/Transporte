using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using TransporteApp.Data;
using TransporteApp.Models;

namespace TransporteApp.ViewModels;

public partial class ConductorViewModel : BaseViewModel
{
    private readonly TransporteDbContext _context;

    [ObservableProperty]
    private ObservableCollection<Conductor> conductores = new();

    [ObservableProperty]
    private Conductor? conductorSeleccionado;

    [ObservableProperty]
    private bool isEditing;

    public ConductorViewModel(TransporteDbContext context)
    {
        _context = context;
        LoadConductoresCommand = new AsyncRelayCommand(LoadConductoresAsync);
        GuardarCommand = new AsyncRelayCommand(GuardarAsync);
        EliminarCommand = new AsyncRelayCommand(EliminarAsync);
        NuevoCommand = new RelayCommand(Nuevo);
        CancelarCommand = new RelayCommand(Cancelar);
    }

    public IAsyncRelayCommand LoadConductoresCommand { get; }
    public IAsyncRelayCommand GuardarCommand { get; }
    public IAsyncRelayCommand EliminarCommand { get; }
    public IRelayCommand NuevoCommand { get; }
    public IRelayCommand CancelarCommand { get; }

    private async Task LoadConductoresAsync()
    {
        try
        {
            var items = await _context.Conductores.ToListAsync();
            Conductores.Clear();
            foreach (var item in items)
            {
                Conductores.Add(item);
            }
        }
        catch (Exception ex)
        {
            MensajeError = $"Error al cargar conductores: {ex.Message}";
        }
    }

    private void Nuevo()
    {
        ConductorSeleccionado = new Conductor
        {
            Nombres = string.Empty,
            Apellidos = string.Empty,
            NumeroDocumento = string.Empty,
            NumeroLicencia = string.Empty,
            FechaVencimientoLicencia = DateTime.Now.AddYears(1),
            Estado = "Activo"
        };
        IsEditing = true;
    }

    private void Cancelar()
    {
        ConductorSeleccionado = null;
        IsEditing = false;
        MensajeError = string.Empty;
    }

    private async Task GuardarAsync()
    {
        if (ConductorSeleccionado == null) return;

        try
        {
            if (ConductorSeleccionado.Id == 0)
            {
                _context.Conductores.Add(ConductorSeleccionado);
            }
            else
            {
                _context.Conductores.Update(ConductorSeleccionado);
            }

            await _context.SaveChangesAsync();
            await LoadConductoresAsync();
            IsEditing = false;
            ConductorSeleccionado = null;
        }
        catch (Exception ex)
        {
            MensajeError = $"Error al guardar: {ex.Message}";
        }
    }

    private async Task EliminarAsync()
    {
        if (ConductorSeleccionado == null || ConductorSeleccionado.Id == 0) return;

        try
        {
            _context.Conductores.Remove(ConductorSeleccionado);
            await _context.SaveChangesAsync();
            await LoadConductoresAsync();
            IsEditing = false;
            ConductorSeleccionado = null;
        }
        catch (Exception ex)
        {
            MensajeError = $"Error al eliminar: {ex.Message}";
        }
    }
}
