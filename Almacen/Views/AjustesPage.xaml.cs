using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Almacen;
using Windows.Storage;
using Almacen.Data;
using Almacen.Models;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using Almacen.Views.View_Almacen;
using Almacen.Estilos_Configuracion.Estilos;



namespace AlmacenApp.Views
{
    public sealed partial class AjustesPage : Page
    {


        private byte[] imagenSeleccionadaBytes = Array.Empty<byte>();
        public ColorDeFondo Tema => ColorDeFondo.Instancia;

        #region contructor
        public AjustesPage()
        {
            
            CargarLogoDesdeBD();
            this.InitializeComponent();
            this.DataContext = this;
            precarga_tema();
        }

        #endregion



        #region carga de datos 

        private async void CargarLogoDesdeBD()
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

                            using (var stream = new InMemoryRandomAccessStream())
                            {
                                await stream.WriteAsync(imagenBytes.AsBuffer());
                                stream.Seek(0);

                                var bitmap = new BitmapImage();
                                await bitmap.SetSourceAsync(stream);

                                Imagen.Source = bitmap;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("No hay logo en la base de datos.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error al cargar el logo desde la base de datos: " + ex.Message);
            }
        }

        #endregion


        #region configuracion de logo




        private async void CambiarImagen_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(openPicker, hwnd);

            Windows.Storage.StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                imagenSeleccionadaBytes = await File.ReadAllBytesAsync(file.Path);

                using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    Imagen.Source = bitmap;
                }

                BtnGuardar.Visibility = Visibility.Visible;
            }
        }






        private void GuardarImagen_Click(object sender, RoutedEventArgs e)
        {
            if (imagenSeleccionadaBytes == null || imagenSeleccionadaBytes.Length == 0)
            {
                Debug.WriteLine("No hay imagen seleccionada para guardar.");
                return;
            }

            var mainWindow = MainWindow.Instance;

            if (mainWindow == null)
            {
                Debug.WriteLine("[ERROR] No se pudo obtener la instancia de `MainWindow`.");
                return;
            }

            Almacen.Data.Perfil_User.GuardarOActualizarLogo(imagenSeleccionadaBytes, mainWindow);

            imagenSeleccionadaBytes = Array.Empty<byte>();

            BtnGuardar.Visibility = Visibility.Collapsed;

            Debug.WriteLine("🚀 Imagen guardada en la base de datos y el botón 'Guardar' ha sido ocultado.");
        }





        #endregion


        #region Configuracion del tema de color

        private async void TemaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string nombreTema = "";

            switch (TemaComboBox.SelectedIndex)
            {
                case 0:
                    ColorDeFondo.Instancia.TemaActual = ColorDeFondo.Tema.Oscuro;
                    ColorDeFondo.Instancia.CambiarTema(ColorDeFondo.Tema.Oscuro);
                    nombreTema = "Oscuro";
                    break;
                case 1:
                    ColorDeFondo.Instancia.TemaActual = ColorDeFondo.Tema.Claro;
                    ColorDeFondo.Instancia.CambiarTema(ColorDeFondo.Tema.Claro);
                    nombreTema = "Claro";
                    break;
                case 2:
                    ColorDeFondo.Instancia.TemaActual = ColorDeFondo.Tema.Sepia;
                    ColorDeFondo.Instancia.CambiarTema(ColorDeFondo.Tema.Sepia);
                    nombreTema = "Sepia";
                    break;
            }

            if (!string.IsNullOrEmpty(nombreTema))
            {
                await Task.Run(() => Perfil_User.ActualizarTemaSeleccionado(nombreTema)); // 🔹 Ejecuta en segundo plano
            }
        }

        private void precarga_tema() 
        {
            string temaGuardado = Perfil_User.ObtenerTemaGuardado();

            // 🔹 Preseleccionar el índice en el ComboBox
            switch (temaGuardado)
            {
                case "Oscuro":
                    TemaComboBox.SelectedIndex = 0;
                    ColorDeFondo.Instancia.TemaActual = ColorDeFondo.Tema.Oscuro;
                    ColorDeFondo.Instancia.CambiarTema(ColorDeFondo.Tema.Oscuro);
                    break;
                case "Claro":
                    TemaComboBox.SelectedIndex = 1;
                    ColorDeFondo.Instancia.TemaActual = ColorDeFondo.Tema.Claro;
                    ColorDeFondo.Instancia.CambiarTema(ColorDeFondo.Tema.Claro);
                    break;
                case "Sepia":
                    TemaComboBox.SelectedIndex = 2;
                    ColorDeFondo.Instancia.TemaActual = ColorDeFondo.Tema.Sepia;
                    ColorDeFondo.Instancia.CambiarTema(ColorDeFondo.Tema.Sepia);
                    break;
            }
        }



        #endregion
    }
}

