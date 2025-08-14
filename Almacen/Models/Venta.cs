using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Almacen.Models
{
    public class Venta
    {
        public int Id { get; set; }  // Identificador único de la venta
        public DateTime Fecha { get; set; }  // Fecha en que se realizó la venta
        public double Total { get; set; }  // Monto total de la venta
    }
}
