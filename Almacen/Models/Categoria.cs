using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlmacenApp.Data;
using System;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AlmacenApp.Models
{
    public class Categoria
    {
        public int Id { get; set; } // Asegúrate de que siempre sea positivo en la lógica de la BD.

        public byte[]? Imagen { get; set; } // Imagen en formato binario (BLOB).

        public string Nombre { get; set; } = string.Empty; // Nombre no debería ser null.

        // Nueva propiedad para manejar la imagen procesada como BitmapImage.
        public BitmapImage? ImagenBitmap { get; set; } // Ayuda a mostrar la imagen en la UI.

        // Constructor opcional para asegurar que se cree con un nombre válido.
        public Categoria(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la categoría no puede estar vacío.", nameof(nombre));

            Nombre = nombre;
        }


        // Constructor vacío necesario para Entity Framework o SQLite
        public Categoria() { }
    }
}
