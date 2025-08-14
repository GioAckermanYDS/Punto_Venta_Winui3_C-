using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Almacen.Models
{
    public class Modelo_RegistroFactura
    {

        public int ID { get; set; }
        public string FechaFactura { get; set; }
        public byte[] PDF { get; set; }
        public string NumeroFactura { get; set; }
        public double PrecioVentaFin { get; set; }



    }
}
