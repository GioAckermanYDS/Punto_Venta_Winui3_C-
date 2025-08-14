using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using AlmacenApp.Views;
using Windows.Networking.NetworkOperators;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
using Almacen.Style.Configuraciones;
using Almacen.Data;
using Almacen.Models;
using Microsoft.Data.Sqlite;
using System.IO;
using Microsoft.UI.Xaml.Media.Imaging;
using Almacen.Estilos_Configuracion.Estilos;
using static Almacen.Estilos_Configuracion.Estilos.ColorDeFondo;








// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Almacen
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        

        public static MainWindow? Instance { get; private set; }       

        public static Frame? AppFrame { get; private set; }
        public static NavigationView? NavigationViewInstance { get; private set; }
        public ColorDeFondo Tema => ColorDeFondo.Instancia;

        public MainWindow()
        {
           
            try
            {                
                this.InitializeComponent();
                Instance = this;
                Carga_Data();
                CargarLogo();

                RootGrid.DataContext = ColorDeFondo.Instancia;
                // Guardar referencia al NavigationView
                NavigationViewInstance = nvSample;

                MainFrame.Navigate(typeof(Login)); // Navegar a la página de inicio
                
                ConfigurarVentana();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("Error durante la inicialización de la ventana principal.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }



        private void Carga_Data() 
        {
            A_Carga_De_Tablas.Creacion_Tablas_BD();
        }
        


        public async void CargarLogo()
        {          
            try
            {
                string dbPath = Path.Combine(A_Ruta_db.Ruta_BD, "BdAlmacen.db");
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Imagen FROM Perfil_User WHERE Id = 1";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            byte[] imagenBytes = (byte[])reader["Imagen"];

                            // Convertir los bytes a un BitmapImage
                            using (MemoryStream ms = new MemoryStream(imagenBytes))
                            {
                                var bitmap = new BitmapImage();
                                await bitmap.SetSourceAsync(ms.AsRandomAccessStream());

                                // 🚀 Se asigna un nuevo `ImageBrush` a `ProfileImage.Fill`
                                var nuevoBrush = new ImageBrush { ImageSource = bitmap };

                                // 🔄 Aquí está la clave: aplicar el nuevo `ImageBrush` directamente al `Ellipse.Fill`
                                ProfileImage.Fill = nuevoBrush;
                               
                            }
                        }
                        else
                        {
                            Debug.WriteLine("[INFO] No se encontró ninguna imagen en la base de datos.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR] Al cargar el logo: " + ex.Message);
            }
        }

        private void NavigateToPage(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (sender is NavigationViewItem navItem && navItem.Tag is string pageTag)
                {
                    if (MainFrame == null)
                    {
                        throw new NullReferenceException("MainFrame no está disponible.");
                    }

                    var transition = new Microsoft.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo();

                    switch (pageTag)
                    {
                        case "Inicio":
                            MainFrame.Navigate(typeof(Login), null, transition);
                            break;
                        case "PuntoDeVenta":
                            MainFrame.Navigate(typeof(VentasPage), null, transition);
                            break;
                        case "Almacen":
                            MainFrame.Navigate(typeof(AlmacenPage), null, transition);
                            break;
                        case "Registro":
                            MainFrame.Navigate(typeof(RegistroPage), null, transition);
                            break;
                        case "Ajustes":
                            MainFrame.Navigate(typeof(AjustesPage), null, transition);
                            break;
                        default:
                            throw new ArgumentException($"Página desconocida: {pageTag}");
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("El objeto `MainFrame` es nulo, evitando la navegación.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("Intento de navegación a una página desconocida.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("Error inesperado en la navegación de página.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }


        private void nvSample_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            try
            {
                // Verifica si el ítem invocado es el botón "Settings"
                if (args.IsSettingsInvoked)
                {
                    // Llama al método NavigateToPage y pasa el valor necesario
                    NavigateToPage(new NavigationViewItem { Tag = "Ajustes" }, new TappedRoutedEventArgs());
                }
                else if (args.InvokedItemContainer is NavigationViewItem navItem)
                {
                    // Maneja otros elementos normalmente
                    NavigateToPage(navItem, new TappedRoutedEventArgs());
                }
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("Referencia nula en `nvSample_ItemInvoked`.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("Error inesperado en `nvSample_ItemInvoked`.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }






        private void NvSample_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
        {
            try
            {
                ProfileImage.Visibility = Visibility.Collapsed;
                WelcomeText.Visibility = Visibility.Collapsed;

                Barra_Navegacion.SetPaneState(false); // Actualiza el estado
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("Referencia nula en NvSample_PaneClosing.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("Error inesperado en NvSample_PaneClosing.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }


        private void NvSample_PaneOpening(NavigationView sender, object args)
        {
            try
            {
                ProfileImage.Visibility = Visibility.Visible;
                WelcomeText.Visibility = Visibility.Visible;

                Barra_Navegacion.SetPaneState(true); // Actualiza el estado
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("Referencia nula en NvSample_PaneOpening.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("Error inesperado en NvSample_PaneOpening.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }

      
        private async void Btn_Exit(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentDialog confirmExitDialog = new ContentDialog
                {
                    Title = "Confirmación",
                    Content = "¿Está seguro de que desea salir?",
                    PrimaryButtonText = "Sí",
                    CloseButtonText = "No",
                    XamlRoot = this.Content.XamlRoot, 
                    Background = Tema.FondoPrimario,
                    Foreground = Tema.ColorDeLetra,
                    BorderBrush = Tema.FondoTersario,
                    BorderThickness = new Thickness(3) 
                };

                // Mostrar el ContentDialog y esperar la respuesta del usuario
                ContentDialogResult result = await confirmExitDialog.ShowAsync();

                // Si el usuario selecciona el botón "Sí" (Primary), cerrar la aplicación
                if (result == ContentDialogResult.Primary)
                {
                    Application.Current.Exit(); // Cierra la aplicación
                }
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("Referencia nula en Btn_Exit.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("Error inesperado en Btn_Exit.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }


        private void ConfigurarVentana()
        {
            try
            {
                // Obtener el identificador de la ventana
                IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);

                // Asegurar que la presentación es válida
                if (appWindow?.Presenter is OverlappedPresenter presenter)
                {
                    // Ocultar completamente los bordes y la barra de título
                    presenter.SetBorderAndTitleBar(false, false);
                    presenter.IsMaximizable = false;
                    presenter.IsMinimizable = false;
                    presenter.IsResizable = false;
                }
                else
                {
                    throw new NullReferenceException("El objeto `appWindow.Presenter` es nulo.");
                }

                // Configurar la ventana en modo pantalla completa sin bordes
                appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("Referencia nula en `ConfigurarVentana`.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE MainWindow]");
                Debug.WriteLine("Error inesperado en `ConfigurarVentana`.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }








    }
}
