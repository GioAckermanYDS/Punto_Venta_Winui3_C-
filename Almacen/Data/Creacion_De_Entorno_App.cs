using System;
using System.IO;

namespace Almacen.Data
{
    public static class Creacion_De_Entorno_App
    {
        public static string CarpetaBase { get; private set; }
        public static string CarpetaBd => Path.Combine(CarpetaBase, "Bd");
        public static string CarpetaAssets => Path.Combine(CarpetaBase, "Assets");
        public static string CarpetaImagenes => Path.Combine(CarpetaAssets, "Imagenes");
        public static string CarpetaCategoria => Path.Combine(CarpetaImagenes, "Categoria");
        public static string CarpetaLogo => Path.Combine(CarpetaImagenes, "Logo");

        static Creacion_De_Entorno_App()
        {
            // Obtener la ruta de "Documentos" del usuario
            string documentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            CarpetaBase = Path.Combine(documentos, "Almacen_Punto de venta");

            // Crear toda la estructura de carpetas si no existe
            CrearCarpetaSiNoExiste(CarpetaBase);
            CrearCarpetaSiNoExiste(CarpetaBd);
            CrearCarpetaSiNoExiste(CarpetaAssets);
            CrearCarpetaSiNoExiste(CarpetaImagenes);
            CrearCarpetaSiNoExiste(CarpetaCategoria);
            CrearCarpetaSiNoExiste(CarpetaLogo);
        }

        private static void CrearCarpetaSiNoExiste(string ruta)
        {
            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta);
            }
        }
    }
}
