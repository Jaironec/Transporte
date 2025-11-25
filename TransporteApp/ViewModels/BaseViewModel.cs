using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TransporteApp.ViewModels;

public partial class BaseViewModel : ObservableValidator
{
    [ObservableProperty]
    private string? mensajeError;

    protected BaseViewModel()
    {
    }
}



