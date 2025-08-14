using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Almacen.Models.Modelo_Validar_Usuario
{
    class Model_Validar_Usuario
    {

        public int ID { get; set; }
        public required string Nombre { get; set; }
        public required string Cedula { get; set; }
        public required string Usuario { get; set; }
        public required string Contraseña { get; set; } // Se recomienda cifrarla antes de guardar

        public int Perfil { get; set; } // 0 = Administrador, 1 = Vendedor

        // Propiedad calculada para devolver el nombre del perfil
        public string NombrePerfil
        {
            get
            {
                return Perfil switch
                {
                    0 => "Administrador",
                    1 => "Vendedor",
                    _ => "Desconocido" // Manejo de valores inesperados
                };
            }
        }

    }


}

