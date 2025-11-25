using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using MaterialDesignThemes.Wpf;
using TransporteApp.ViewModels;

namespace TransporteApp.Views;

public partial class MainWindow : Window
{
    private bool _sidebarCollapsed = false;

    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        DataContext = mainViewModel;
        
        // Cargar datos iniciales si es necesario
        Loaded += MainWindow_Loaded;
    }

    private MainViewModel ViewModel => (MainViewModel)DataContext;

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Si el ViewModel actual es Dashboard, cargar sus datos
        if (ViewModel.CurrentViewModel is DashboardViewModel dashboardVM)
        {
            try
            {
                await dashboardVM.LoadDashboardDataCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar los datos del dashboard:\n\n{ex.Message}\n\n" +
                    "La aplicación continuará funcionando, pero los datos no se cargarán.\n" +
                    "Verifique la conexión a PostgreSQL.",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }

    private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        _sidebarCollapsed = !_sidebarCollapsed;

        var widthAnimation = new DoubleAnimation
        {
            Duration = new Duration(TimeSpan.FromSeconds(0.3)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };

        if (_sidebarCollapsed)
        {
            widthAnimation.To = 60;
            ToggleSidebarButton.Content = new PackIcon { Kind = PackIconKind.ChevronRight };
            // Ocultar texto de los items del menú
            foreach (ListBoxItem item in MenuListBox.Items)
            {
                var stackPanel = item.Content as StackPanel;
                if (stackPanel != null && stackPanel.Children.Count > 1)
                {
                    stackPanel.Children[1].Visibility = Visibility.Collapsed;
                }
            }
        }
        else
        {
            widthAnimation.To = 280;
            ToggleSidebarButton.Content = new PackIcon { Kind = PackIconKind.ChevronLeft };
            // Mostrar texto de los items del menú
            foreach (ListBoxItem item in MenuListBox.Items)
            {
                var stackPanel = item.Content as StackPanel;
                if (stackPanel != null && stackPanel.Children.Count > 1)
                {
                    stackPanel.Children[1].Visibility = Visibility.Visible;
                }
            }
        }

        Sidebar.BeginAnimation(WidthProperty, widthAnimation);
    }

    private void ToggleTheme_Click(object sender, RoutedEventArgs e)
    {
        // Cambiar entre tema claro y oscuro
        var paletteHelper = new PaletteHelper();
        var theme = paletteHelper.GetTheme();
        
        if (theme.GetBaseTheme() == BaseTheme.Dark)
        {
            theme.SetBaseTheme(Theme.Light);
        }
        else
        {
            theme.SetBaseTheme(Theme.Dark);
        }
        
        paletteHelper.SetTheme(theme);
    }
}
