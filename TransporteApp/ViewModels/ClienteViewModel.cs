using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using TransporteApp.Data;
using TransporteApp.Models;

namespace TransporteApp.ViewModels;

public partial class ClienteViewModel : BaseViewModel
{
    private readonly TransporteDbContext _context;

    [ObservableProperty]
    private ObservableCollection<Cliente> clientes = new();

    [ObservableProperty]
    private Cliente? clienteSeleccionado;

    [ObservableProperty]
    private bool isEditing;

    public ClienteViewModel(TransporteDbContext context)
    {
        _context = context;
        LoadClientesCommand = new AsyncRelayCommand(LoadClientesAsync);
        GuardarCommand = new AsyncRelayCommand(GuardarAsync);
        EliminarCommand = new AsyncRelayCommand(EliminarAsync);
        NuevoCommand = new RelayCommand(Nuevo);
        CancelarCommand = new RelayCommand(Cancelar);
    }

    public IAsyncRelayCommand LoadClientesCommand { get; }
    public IAsyncRelayCommand GuardarCommand { get; }
    public IAsyncRelayCommand EliminarCommand { get; }
    public IRelayCommand NuevoCommand { get; }
    public IRelayCommand CancelarCommand { get; }

    private async Task LoadClientesAsync()
    {
        try
        {
            var items = await _context.Clientes.ToListAsync();
            Clientes.Clear();
            foreach (var item in items)
            {
                Clientes.Add(item);
            }
        }
        catch (Exception ex)
        {
            MensajeError = $"Error al cargar clientes: {ex.Message}";
        }
    }

    private void Nuevo()
    {
        ClienteSeleccionado = new Cliente
        {
            RazonSocial = string.Empty,
            Estado = "Activo"
        };
        IsEditing = true;
    }

    private void Cancelar()
    {
        ClienteSeleccionado = null;
        IsEditing = false;
        MensajeError = string.Empty;
    }

    private async Task GuardarAsync()
    {
        if (ClienteSeleccionado == null) return;

        try
        {
            if (ClienteSeleccionado.Id == 0)
            {
                _context.Clientes.Add(ClienteSeleccionado);
            }
            else
            {
                _context.Clientes.Update(ClienteSeleccionado);
            }

            await _context.SaveChangesAsync();
            await LoadClientesAsync();
            IsEditing = false;
            ClienteSeleccionado = null;
        }
        catch (Exception ex)
        {
            MensajeError = $"Error al guardar: {ex.Message}";
        }
    }

    private async Task EliminarAsync()
    {
        if (ClienteSeleccionado == null || ClienteSeleccionado.Id == 0) return;

        try
        {
            _context.Clientes.Remove(ClienteSeleccionado);
            await _context.SaveChangesAsync();
            await LoadClientesAsync();
            IsEditing = false;
            ClienteSeleccionado = null;
        }
        catch (Exception ex)
        {
            MensajeError = $"Error al eliminar: {ex.Message}";
        }
    }
}
