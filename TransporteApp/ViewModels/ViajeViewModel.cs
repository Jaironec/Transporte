using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using TransporteApp.Data;
using TransporteApp.Models;

namespace TransporteApp.ViewModels;

public partial class ViajeViewModel : BaseViewModel
{
    private readonly TransporteDbContext _context;

    public ISnackbarMessageQueue SnackbarMessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(4));

    [ObservableProperty]
    private Viaje? viajeSeleccionado;

    // Propiedades Wrapper para Validación
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "El vehículo es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un vehículo")]
    private int selectedVehiculoId;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "El conductor es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un conductor")]
    private int selectedConductorId;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "El cliente es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente")]
    private int selectedClienteId;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "El origen es obligatorio")]
    [MinLength(3, ErrorMessage = "El origen debe tener al menos 3 caracteres")]
    private string origen = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "El destino es obligatorio")]
    [MinLength(3, ErrorMessage = "El destino debe tener al menos 3 caracteres")]
    private string destino = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "El flete es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El flete debe ser mayor a 0")]
    private decimal flete;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "El pago al conductor es obligatorio")]
    [Range(0, double.MaxValue, ErrorMessage = "El pago no puede ser negativo")]
    private decimal pagoConductor;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Los litros son obligatorios")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Los litros deben ser mayores a 0")]
    private decimal cantidadLitros;

    [ObservableProperty]
    private DateTime fechaSalida = DateTime.Now;

    [ObservableProperty]
    private string estado = "Programado";

    // Colecciones
    [ObservableProperty]
    private ObservableCollection<Viaje> viajes = new();

    [ObservableProperty]
    private ObservableCollection<Vehiculo> vehiculos = new();

    [ObservableProperty]
    private ObservableCollection<Conductor> conductores = new();

    [ObservableProperty]
    private ObservableCollection<Cliente> clientes = new();

    [ObservableProperty]
    private ObservableCollection<GastoOperativo> gastos = new();

    // Métricas
    [ObservableProperty]
    private decimal utilidadBruta;

    [ObservableProperty]
    private decimal totalGastos;

    [ObservableProperty]
    private decimal utilidadNeta;

    [ObservableProperty]
    private decimal rendimientoCombustible;

    [ObservableProperty]
    private bool isLoading;

    // Nuevo Gasto
    [ObservableProperty]
    private string nuevoGastoTipo = "Combustible";
    
    [ObservableProperty]
    private string nuevoGastoDescripcion = "";
    
    [ObservableProperty]
    private decimal nuevoGastoMonto;

    public ViajeViewModel(TransporteDbContext context)
    {
        _context = context;
        
        GuardarViajeCommand = new AsyncRelayCommand(RegistrarViajeCompletoAsync, CanGuardarViaje);
        EliminarViajeCommand = new AsyncRelayCommand(EliminarViajeAsync, () => ViajeSeleccionado != null && !IsLoading);
        AgregarGastoCommand = new AsyncRelayCommand(AgregarGastoAsync, () => ViajeSeleccionado != null && !IsLoading);
        EliminarGastoCommand = new AsyncRelayCommand<int>(EliminarGastoAsync, id => !IsLoading);
        LoadViajesCommand = new AsyncRelayCommand(CargarDatosAsync);
        NuevoViajeCommand = new RelayCommand(PrepararNuevoViaje);
    }

    public IAsyncRelayCommand GuardarViajeCommand { get; }
    public IAsyncRelayCommand EliminarViajeCommand { get; }
    public IAsyncRelayCommand AgregarGastoCommand { get; }
    public IAsyncRelayCommand<int> EliminarGastoCommand { get; }
    public IAsyncRelayCommand LoadViajesCommand { get; }
    public IRelayCommand NuevoViajeCommand { get; }

    private bool CanGuardarViaje()
    {
        return !HasErrors && !IsLoading;
    }

    partial void OnSelectedVehiculoIdChanged(int value) 
    {
        ValidateProperty(value, nameof(SelectedVehiculoId));
        GuardarViajeCommand.NotifyCanExecuteChanged();
    }
    partial void OnSelectedConductorIdChanged(int value) 
    {
        ValidateProperty(value, nameof(SelectedConductorId));
        GuardarViajeCommand.NotifyCanExecuteChanged();
    }
    partial void OnSelectedClienteIdChanged(int value) 
    {
        ValidateProperty(value, nameof(SelectedClienteId));
        GuardarViajeCommand.NotifyCanExecuteChanged();
    }
    partial void OnOrigenChanged(string value) 
    {
        ValidateProperty(value, nameof(Origen));
        GuardarViajeCommand.NotifyCanExecuteChanged();
    }
    partial void OnDestinoChanged(string value) 
    {
        ValidateProperty(value, nameof(Destino));
        GuardarViajeCommand.NotifyCanExecuteChanged();
    }
    partial void OnFleteChanged(decimal value) 
    {
        ValidateProperty(value, nameof(Flete));
        CalcularMetricas();
        GuardarViajeCommand.NotifyCanExecuteChanged();
    }
    partial void OnPagoConductorChanged(decimal value) 
    {
        ValidateProperty(value, nameof(PagoConductor));
        CalcularMetricas();
        GuardarViajeCommand.NotifyCanExecuteChanged();
    }

    partial void OnViajeSeleccionadoChanged(Viaje? value)
    {
        if (value != null)
        {
            // Mapear Modelo a ViewModel
            SelectedVehiculoId = value.VehiculoId;
            SelectedConductorId = value.ConductorId;
            SelectedClienteId = value.ClienteId;
            Origen = value.Origen;
            Destino = value.Destino;
            Flete = value.Flete;
            PagoConductor = value.PagoConductor;
            FechaSalida = value.FechaSalida;
            CantidadLitros = value.CantidadLitros;
            Estado = value.Estado;

            _ = CargarGastosAsync(value.Id);
        }
        else
        {
            LimpiarFormulario();
        }
        GuardarViajeCommand.NotifyCanExecuteChanged();
        EliminarViajeCommand.NotifyCanExecuteChanged();
        AgregarGastoCommand.NotifyCanExecuteChanged();
    }

    private void LimpiarFormulario()
    {
        SelectedVehiculoId = 0;
        SelectedConductorId = 0;
        SelectedClienteId = 0;
        Origen = string.Empty;
        Destino = string.Empty;
        Flete = 0;
        PagoConductor = 0;
        FechaSalida = DateTime.Now;
        CantidadLitros = 0;
        Estado = "Programado";
        Gastos.Clear();
        UtilidadBruta = 0;
        TotalGastos = 0;
        UtilidadNeta = 0;
        RendimientoCombustible = 0;
        ClearErrors();
    }

    protected override void OnErrorsChanged(DataErrorsChangedEventArgs e)
    {
        base.OnErrorsChanged(e);
        GuardarViajeCommand.NotifyCanExecuteChanged();
    }

    private void PrepararNuevoViaje()
    {
        ViajeSeleccionado = new Viaje
        {
            NumeroViaje = $"V-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}",
            FechaSalida = DateTime.Now,
            VehiculoId = 0, ConductorId = 0, ClienteId = 0,
            Origen = "", Destino = "",
            CantidadLitros = 0, Flete = 0, PagoConductor = 0,
            Estado = "Programado"
        };
        // El OnViajeSeleccionadoChanged se encargará de mapear (y limpiar/validar)
    }

    private async Task CargarDatosAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            IsLoading = true;
            MensajeError = null;

            await Task.WhenAll(
                CargarViajesAsync(),
                CargarVehiculosAsync(),
                CargarConductoresAsync(),
                CargarClientesAsync()
            );
        }, "No se pudo cargar los datos desde la base de datos.");
    }

    private async Task CargarViajesAsync()
    {
        var lista = await _context.Viajes
            .Include(v => v.Vehiculo)
            .Include(v => v.Conductor)
            .Include(v => v.Cliente)
            .Include(v => v.Gastos)
            .OrderByDescending(v => v.FechaSalida)
            .ToListAsync();

        Viajes.Clear();
        foreach (var v in lista) Viajes.Add(v);
    }

    private async Task CargarVehiculosAsync()
    {
        var lista = await _context.Vehiculos.Where(v => v.Estado == "Activo").ToListAsync();
        Vehiculos.Clear();
        foreach (var v in lista) Vehiculos.Add(v);
    }

    private async Task CargarConductoresAsync()
    {
        var lista = await _context.Conductores.Where(c => c.Estado == "Activo").ToListAsync();
        Conductores.Clear();
        foreach (var c in lista) Conductores.Add(c);
    }

    private async Task CargarClientesAsync()
    {
        var lista = await _context.Clientes.Where(c => c.Estado == "Activo").ToListAsync();
        Clientes.Clear();
        foreach (var c in lista) Clientes.Add(c);
    }

    private async Task CargarGastosAsync(int viajeId)
    {
        if (viajeId == 0) 
        {
            Gastos.Clear();
            return;
        }

        var lista = await _context.GastosOperativos
            .Where(g => g.ViajeId == viajeId)
            .OrderByDescending(g => g.Fecha)
            .ToListAsync();

        Gastos.Clear();
        foreach (var g in lista) Gastos.Add(g);

        CalcularMetricas();
    }

    private void CalcularMetricas()
    {
        UtilidadBruta = Flete - PagoConductor;
        TotalGastos = Gastos.Sum(g => g.Monto);
        UtilidadNeta = Flete - (PagoConductor + TotalGastos);

        RendimientoCombustible = CantidadLitros > 0
            ? Math.Round(UtilidadNeta / CantidadLitros, 2)
            : 0;
    }

    partial void OnCantidadLitrosChanged(decimal value)
    {
        ValidateProperty(value, nameof(CantidadLitros));
        CalcularMetricas();
        GuardarViajeCommand.NotifyCanExecuteChanged();
    }

    private async Task RegistrarViajeCompletoAsync()
    {
        ValidateAllProperties();
        if (HasErrors)
        {
            MensajeError = "Por favor corrija los errores antes de guardar.";
            SnackbarMessageQueue.Enqueue(MensajeError);
            return;
        }

        if (ViajeSeleccionado == null) return;

        await ExecuteSafeAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            IsLoading = true;
            MensajeError = null;

            // Actualizar Modelo desde ViewModel
            ViajeSeleccionado.VehiculoId = SelectedVehiculoId;
            ViajeSeleccionado.ConductorId = SelectedConductorId;
            ViajeSeleccionado.ClienteId = SelectedClienteId;
            ViajeSeleccionado.Origen = Origen;
            ViajeSeleccionado.Destino = Destino;
            ViajeSeleccionado.Flete = Flete;
            ViajeSeleccionado.PagoConductor = PagoConductor;
            ViajeSeleccionado.FechaSalida = FechaSalida;
            ViajeSeleccionado.CantidadLitros = CantidadLitros;
            ViajeSeleccionado.Estado = Estado;

            if (ViajeSeleccionado.Id == 0)
            {
                _context.Viajes.Add(ViajeSeleccionado);
            }
            else
            {
                _context.Viajes.Update(ViajeSeleccionado);
            }
            await _context.SaveChangesAsync();

            var vehiculo = await _context.Vehiculos.FindAsync(SelectedVehiculoId);
            if (vehiculo != null && Estado == "EnCurso")
            {
                vehiculo.Estado = "Activo";
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await CargarViajesAsync();
            SnackbarMessageQueue.Enqueue("Viaje registrado exitosamente.");
        }, "No se pudo registrar el viaje. Verifique la conexión a la base de datos.");
    }

    private async Task EliminarViajeAsync()
    {
        if (ViajeSeleccionado == null || ViajeSeleccionado.Id == 0) return;

        if (MessageBox.Show("¿Confirma la eliminación del viaje?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        await ExecuteSafeAsync(async () =>
        {
            IsLoading = true;
            _context.Viajes.Remove(ViajeSeleccionado);
            await _context.SaveChangesAsync();
            await CargarViajesAsync();
            ViajeSeleccionado = null;
            SnackbarMessageQueue.Enqueue("Viaje eliminado correctamente.");
        }, "No se pudo eliminar el viaje seleccionado.");
    }

    private async Task AgregarGastoAsync()
    {
        if (ViajeSeleccionado == null || ViajeSeleccionado.Id == 0)
        {
            MensajeError = "Guarde el viaje antes de agregar gastos.";
            SnackbarMessageQueue.Enqueue(MensajeError);
            return;
        }

        if (NuevoGastoMonto <= 0)
        {
            MensajeError = "El monto debe ser mayor a 0.";
            SnackbarMessageQueue.Enqueue(MensajeError);
            return;
        }

        await ExecuteSafeAsync(async () =>
        {
            var gasto = new GastoOperativo
            {
                ViajeId = ViajeSeleccionado.Id,
                Tipo = NuevoGastoTipo,
                Descripcion = NuevoGastoDescripcion,
                Monto = NuevoGastoMonto,
                Fecha = DateTime.Now
            };

            _context.GastosOperativos.Add(gasto);
            await _context.SaveChangesAsync();

            NuevoGastoDescripcion = "";
            NuevoGastoMonto = 0;

            await CargarGastosAsync(ViajeSeleccionado.Id);
            SnackbarMessageQueue.Enqueue("Gasto agregado.");
        }, "No se pudo agregar el gasto. Intente nuevamente.");
    }

    private async Task EliminarGastoAsync(int id)
    {
        await ExecuteSafeAsync(async () =>
        {
            var gasto = await _context.GastosOperativos.FindAsync(id);
            if (gasto != null)
            {
                _context.GastosOperativos.Remove(gasto);
                await _context.SaveChangesAsync();
                if (ViajeSeleccionado != null)
                {
                    await CargarGastosAsync(ViajeSeleccionado.Id);
                }
                SnackbarMessageQueue.Enqueue("Gasto eliminado.");
            }
        }, "No se pudo eliminar el gasto seleccionado.");
    }

    private async Task ExecuteSafeAsync(Func<Task> action, string friendlyError)
    {
        try
        {
            await action();
        }
        catch (DbUpdateException ex)
        {
            await HandleErrorAsync(friendlyError, ex);
        }
        catch (InvalidOperationException ex)
        {
            await HandleErrorAsync(friendlyError, ex);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(friendlyError, ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task HandleErrorAsync(string friendlyError, Exception ex)
    {
        MensajeError = $"{friendlyError}\nDetalle: {ex.Message}";
        SnackbarMessageQueue.Enqueue(MensajeError);
        return Task.CompletedTask;
    }
}
