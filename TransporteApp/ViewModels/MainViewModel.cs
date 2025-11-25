using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace TransporteApp.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly ViajeViewModel _viajeViewModel;
    private readonly VehiculoViewModel _vehiculoViewModel;
    private readonly ConductorViewModel _conductorViewModel;
    private readonly ClienteViewModel _clienteViewModel;

    [ObservableProperty]
    private object currentViewModel;

    [ObservableProperty]
    private string currentTitle = "Dashboard";

    public MainViewModel(
        DashboardViewModel dashboardViewModel,
        ViajeViewModel viajeViewModel,
        VehiculoViewModel vehiculoViewModel,
        ConductorViewModel conductorViewModel,
        ClienteViewModel clienteViewModel)
    {
        _dashboardViewModel = dashboardViewModel;
        _viajeViewModel = viajeViewModel;
        _vehiculoViewModel = vehiculoViewModel;
        _conductorViewModel = conductorViewModel;
        _clienteViewModel = clienteViewModel;
        
        // Default view
        CurrentViewModel = _dashboardViewModel;
        
        NavigateCommand = new RelayCommand<string>(Navigate);
    }

    public ICommand NavigateCommand { get; }

    private void Navigate(string? destination)
    {
        switch (destination)
        {
            case "Dashboard":
                CurrentViewModel = _dashboardViewModel;
                CurrentTitle = "Dashboard Principal";
                _dashboardViewModel.LoadDashboardDataCommand.Execute(null);
                break;
            case "Viajes":
                CurrentViewModel = _viajeViewModel;
                CurrentTitle = "Gestión de Viajes";
                _viajeViewModel.LoadViajesCommand.Execute(null);
                break;
            case "Vehiculos":
                CurrentViewModel = _vehiculoViewModel;
                CurrentTitle = "Gestión de Vehículos";
                _vehiculoViewModel.LoadVehiculosCommand.Execute(null);
                break;
            case "Conductores":
                CurrentViewModel = _conductorViewModel;
                CurrentTitle = "Gestión de Conductores";
                _conductorViewModel.LoadConductoresCommand.Execute(null);
                break;
            case "Clientes":
                CurrentViewModel = _clienteViewModel;
                CurrentTitle = "Gestión de Clientes";
                _clienteViewModel.LoadClientesCommand.Execute(null);
                break;
            default:
                CurrentViewModel = _dashboardViewModel;
                CurrentTitle = "Dashboard Principal";
                break;
        }
    }
}
