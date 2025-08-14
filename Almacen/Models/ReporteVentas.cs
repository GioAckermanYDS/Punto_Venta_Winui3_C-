using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Almacen.Models
{
    public class ReporteVentas
    {
        public DateTime Fecha { get; set; }  // Fecha de la venta
        public double TotalVendido { get; set; }  // Suma total de las ventas en el día
        public int NumeroVentas { get; set; }  // Cantidad de ventas en el día
    }
}

