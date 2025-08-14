using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Almacen.Estilos_Configuracion.Estilos;
using Almacen.Data;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using Windows.Storage.Streams;
using Windows.Networking.NetworkOperators;
using Almacen.Data.Validar_Ingreso;
using Almacen.Models.Validar_Usuario;




// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Almacen.Views.Login_Uauario
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class View_Login_Usuarios : Page
    {

        public ColorDeFondo Tema => ColorDeFondo.Instancia;
       


        public View_Login_Usuarios()
        {
            this.InitializeComponent();
            this.DataContext = this;
            CargarLogo();
            Cargar_Datos();
            

        }

        private void Cargar_Datos()
        {

            Login_NombreApp.Text = Perfil_User.Obtener_NombreApp(); // Muestra el nombre en el TextBox
            Text_Usuario.Text = Control_Usuarios.Obtener_Ultimo_Usuario();
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
                    command.CommandText = "SELECT Imagen FROM Perfil_User_Img WHERE Id = 1";

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

        private async void Btn_IniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            Control_Usuarios.Create_BD_Ultimo_Inicio_Usuarios();
            string usuarioIngresado = Text_Usuario.Text.Trim();
            string contraseñaIngresada = Text_Contraseña.Text.Trim();

            // ✅ Usamos el método correcto que extrae desde la tabla Usuarios
            var usuarios = Control_Usuarios.ObtenerPerfilesUsuarios();

            // ✅ Verificamos si hay coincidencia
            var usuarioAutenticado = usuarios.FirstOrDefault(u =>
                u.Usuario == usuarioIngresado && u.Contraseña == contraseñaIngresada);

            if (usuarioAutenticado != null)
            {
                Debug.WriteLine("[INFO] Inicio de sesión exitoso. Usuario autenticado.");

                // ✅ Guardar el usuario autenticado como último inicio
                Control_Usuarios.Insertar_Ultimo_Usuario(
                    tipoPerfil: usuarioAutenticado.Perfil.ToString(),
                    nombre: usuarioAutenticado.Nombre,
                    cedula: usuarioAutenticado.Cedula,
                    usuario: usuarioAutenticado.Usuario
                );

                // Abre la ventana principal
                var nuevaVentana = new MainWindow();
                nuevaVentana.Activate();

                // Cierra la ventana actual
                App.MainWindow?.Close();
                App.MainWindow = nuevaVentana;
            }
            else
            {
                Debug.WriteLine("[ERROR] Usuario o contraseña incorrectos.");

                var dialog = new ContentDialog
                {
                    Title = "Error de inicio de sesión",
                    Content = "Usuario o contraseña incorrectos.\nPor favor, intenta nuevamente.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.XamlRoot,
                    Background = Tema.FondoPrimario,
                    Foreground = Tema.ColorDeLetra,
                    BorderBrush = Tema.FondoTersario,
                    BorderThickness = new Thickness(3)
                };

                await dialog.ShowAsync();
            }
        }

        private async void Btn_Cancelar_Inicio_Click(object sender, RoutedEventArgs e)
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
                    Microsoft.UI.Xaml.Application.Current.Exit();

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


    }
}
