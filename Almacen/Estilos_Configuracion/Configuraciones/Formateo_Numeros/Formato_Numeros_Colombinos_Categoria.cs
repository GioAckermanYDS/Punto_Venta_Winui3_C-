using Microsoft.UI.Xaml.Data;
using System;
using System.Diagnostics;
using System.Globalization;

namespace Almacen.Estilos_Configuracion.Configuraciones.Formateo_Numeros
{
    public class Formato_Numeros_Colombinos : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                decimal numero = System.Convert.ToDecimal(value);

                var culturaColombiana = new CultureInfo("es-CO")
                {
                    NumberFormat =
                    {
                        CurrencyDecimalDigits = 0,
                        NumberGroupSeparator = ".",
                        CurrencySymbol = ""
                    }
                };

                string resultado = numero.ToString("N0", culturaColombiana);
                return $"{resultado} $";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error en Formato_Numeros_Colombinos: {ex.Message}");
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
