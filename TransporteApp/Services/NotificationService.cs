using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;

namespace TransporteApp.Services;

public class NotificationService
{
    public static async Task ShowSnackbarAsync(string message, bool isError = false)
    {
        var snackbar = new SnackbarMessage
        {
            Content = message,
            ActionContent = "OK"
        };

        await DialogHost.Show(snackbar, "MainDialogHost");
    }

    public static void ShowToast(string message, bool isError = false)
    {
        // Implementación de toast usando MaterialDesign
        var snackbarMessage = new SnackbarMessage
        {
            Content = message,
            ActionContent = "Cerrar"
        };

        // Esto se puede expandir para usar un SnackbarMessageQueue
    }
}

