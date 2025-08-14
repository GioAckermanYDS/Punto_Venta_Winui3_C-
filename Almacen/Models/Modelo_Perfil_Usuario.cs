using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Data.Sqlite;
using Almacen.Data;
using System.Diagnostics;

namespace Almacen.Models
{
    public class Modelo_Perfil_Usuario : INotifyPropertyChanged
    {

        #region Configuracion de imagen

        public int Id { get; set; }

        private byte[] _imagenBytes = Array.Empty<byte>();

        public byte[] ImagenBytes
        {
            get => _imagenBytes;
            set
            {
                if (_imagenBytes != value)
                {
                    _imagenBytes = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ImagenBitmap)); // 🚀 Notificar que la imagen cambió
                }
            }
        }

        public BitmapImage ImagenBitmap
        {
            get
            {
                if (ImagenBytes == null || ImagenBytes.Length == 0)
                    return new BitmapImage(); // Retorna un BitmapImage vacío en lugar de null

                var bitmap = new BitmapImage();
                using (var stream = new MemoryStream(ImagenBytes))
                {
                    stream.Position = 0;
                    bitmap.SetSource(stream.AsRandomAccessStream());
                }
                return bitmap;
            }
        }


        // 🔄 Evento para actualizar la UI automáticamente
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? ""));
        }


        // 📥 Constructor para cargar la imagen automáticamente desde la base de datos


        public Modelo_Perfil_Usuario()
        {
            CargarImagenDesdeBD();
        }

        // 🔄 Método para cargar la imagen desde la base de datos
        public void CargarImagenDesdeBD()
        {
            try
            {
                string dbPath = Path.Combine(A_Ruta_db.Ruta_BD, "BdAlmacen.db");
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Imagen FROM Perfil_User WHERE Id = 1";


                    object? result = command.ExecuteScalar();
                    if (result is byte[] imageBytes && imageBytes.Length > 0)
                    {
                        ImagenBytes = imageBytes;
                        Debug.WriteLine("🚀 Imagen cargada desde la base de datos.");
                    }
                    else
                    {
                        Debug.WriteLine("[INFO] No se encontró ninguna imagen en la base de datos.");
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR] Al cargar la imagen desde la base de datos: " + ex.Message);
            }
        }

        // 📥 Método para actualizar la imagen desde los datos de la base de datos
        public void ActualizarImagen(byte[] nuevaImagen)
        {
            if (nuevaImagen != null && nuevaImagen.Length > 0)
            {
                ImagenBytes = nuevaImagen; // 🚀 Se actualiza la imagen en tiempo real
                Debug.WriteLine("🚀 Imagen del modelo actualizada.");
            }
            else
            {
                Debug.WriteLine("[INFO] La imagen proporcionada para actualizar es nula o vacía.");
            }
        }

        #endregion

        #region Configuracio del tema fondo


        #endregion

        #region Configuracio Nombre


        #endregion

    }
}
