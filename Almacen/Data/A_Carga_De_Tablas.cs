using Almacen.Estilos_Configuracion.Estilos;
using Almacen.Models;
using AlmacenApp.Data;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.NetworkOperators;

namespace Almacen.Data
{
    class A_Carga_De_Tablas
    {
        public static void Creacion_Tablas_BD()
        {
            Bd_Producto.Create_BD_Producto();
            //Bd_AlertaStock.CrearAlerta( alertaStock);
            Bd_Categoria.Create_BD_Categoria();
            Perfil_User.Crear_BD_TablaImg();
            Perfil_User.Crear_BD_TablaTemasFondo();
            CargarTemaDesdeBaseDatos();
        }

        public static void CargarTemaDesdeBaseDatos()
        {
            string temaGuardado = Perfil_User.ObtenerTemaGuardado(); // 🔹 Recuperar el tema de la base de datos

            switch (temaGuardado)
            {
                case "Oscuro":
                    ColorDeFondo.Instancia.TemaActual = ColorDeFondo.Tema.Oscuro;
                    ColorDeFondo.Instancia.CambiarTema(ColorDeFondo.Tema.Oscuro);
                    break;
                case "Claro":
                    ColorDeFondo.Instancia.TemaActual = ColorDeFondo.Tema.Claro;
                    ColorDeFondo.Instancia.CambiarTema(ColorDeFondo.Tema.Claro);
                    break;
                case "Sepia":
                    ColorDeFondo.Instancia.TemaActual = ColorDeFondo.Tema.Sepia;
                    ColorDeFondo.Instancia.CambiarTema(ColorDeFondo.Tema.Sepia);
                    break;
            }

          
        }

      
    }
}
