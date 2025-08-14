using System;
using System.IO;

namespace Almacen.Data
{
    public static class A_Ruta_db
    {
        // Usamos la ruta de la carpeta Bd definida en tu clase de entorno
        public static string Ruta_BD => Path.Combine(Creacion_De_Entorno_App.CarpetaBd, "BdAlmacen.db");

        static A_Ruta_db()
        {
            // Aseguramos que la carpeta Bd exista (ya lo hace tu clase de entorno, pero no sobra)
            Directory.CreateDirectory(Creacion_De_Entorno_App.CarpetaBd);

            // Ruta del archivo .db original que se incluye junto al .exe (por si quieres copiar una base de datos predefinida)
            string dbOrigen = Path.Combine(AppContext.BaseDirectory, "BdAlmacen.db");

            // Si no existe la base en la carpeta final, y sí existe en el origen, la copiamos
            if (!File.Exists(Ruta_BD) && File.Exists(dbOrigen))
            {
                File.Copy(dbOrigen, Ruta_BD);
            }
        }
    }
}
