using System;
using System.Globalization;
using AlmacenApp.Data;
using Almacen.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Foundation;
using System.Collections.Generic;
using Almacen.Data;
using Microsoft.UI.Xaml.Media; // Para SolidColorBrush y Microsoft.UI.Colors
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Windows.UI;
using Windows.System;
using Almacen.Style;
using AlmacenApp.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;
using Almacen.Estilos_Configuracion.Estilos;


namespace Almacen.Views.View_Almacen
{
    public sealed partial class View_Agregar_Producto : Page
    {

        private Producto producto = new Producto();

        /// <summary>
        /// Evento que se invoca cuando un producto es guardado o la ventana se cierra.
        /// </summary>
        public event Action? ProductoGuardado;

        /// <summary>
        /// ID de la categoría del producto.
        /// </summary>
        private int CategoriaId { get; set; }

        public ColorDeFondo Tema => ColorDeFondo.Instancia;

        /// <summary>
        /// Constructor de la clase View_Agregar_Producto.
        /// </summary>
        /// <param name="categoriaId">ID de la categoría a la que pertenece el producto.</param>
        public View_Agregar_Producto(int categoriaId)
        {
            this.InitializeComponent();
            this.DataContext = this;
            CategoriaId = categoriaId;
            MostrarOcultarCampos();
            
            Debug.WriteLine("[INFO] Constructor inicializado correctamente. View_Agregar_Producto");
        }

        /// <summary>
        /// Guarda el nuevo producto en la base de datos.
        /// </summary>
        private void Btn_Guardar(object sender, RoutedEventArgs e)
        {
            try
            {
                // Eliminar separadores de miles antes de la conversión
                string nombreProducto = NombreTextBox.Text;
                string descripcionProducto = DescripcionTextBox.Text;              

                int precioCompraProducto = int.TryParse(PrecioCompraTextBox.Text.Replace(".", "").Trim(), out var precioCompra) ? precioCompra : 0;
                double precioFijoProducto = double.TryParse(PrecioFijoTextBox.Text.Replace(".", "").Trim(), out var precioFijo) ? precioFijo : 0;
                double porcentajeGananciaProducto = double.TryParse(PorcentajeGananciaTextBox.Text.Replace(".", "").Trim(), out var porcentajeGanancia) ? porcentajeGanancia : 0;

                // Determinar si se usa Precio Fijo o Porcentaje
                bool eleccionCalculo = PrecioFijoRadioButton.IsChecked == true;

                // Crear el producto con las propiedades necesarias
                Producto nuevoProducto = new Producto
                {
                    CategoriaId = CategoriaId,
                    Nombre = nombreProducto,
                    Descripcion = descripcionProducto,
                    PrecioCompra = precioCompraProducto,
                    Cantidad = int.TryParse(CantidadTextBox.Text.Trim(), out var cantidad) ? cantidad : 0,
                    Precio_Fijo = eleccionCalculo ? precioFijoProducto : 0, // Si usa Precio Fijo, asignar el valor; de lo contrario, 0
                    Precio_Porcentaje = !eleccionCalculo ? porcentajeGananciaProducto : 0, // Si usa Porcentaje, asignar el valor; de lo contrario, 0
                    Eleccion_Calculo = eleccionCalculo // Determinar el método de cálculo seleccionado
                };

                // Guardar el producto en la base de datos
                int productoId = Bd_Producto.CreateProducto(nuevoProducto);

                // Notificar éxito
                ProductoGuardado?.Invoke();

                // Regresar a la vista anterior
                var parentFrame = this.Parent as Frame;
                if (parentFrame != null && parentFrame.CanGoBack)
                {
                    parentFrame.GoBack();
                    ProductoGuardado?.Invoke(); 
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error al guardar el producto:" +
                    $" Metodo Btn_Guardar" +
                    $" View_Agregar_Producto {ex.Message}");
            }
        }




        /// <summary>
        /// Cierra la vista actual y regresa a la vista principal.
        /// </summary>
        private void Btn_Cerrar(object sender, RoutedEventArgs e)
        {
            var parent = this.Parent as FrameworkElement;

            while (parent != null && !(parent is Grid))
            {
                parent = parent.Parent as FrameworkElement;
            }

            if (parent is Grid parentGrid)
            {
                // Colapsar el grid
                parentGrid.Visibility = Visibility.Collapsed;

                // Buscar el Frame dentro del Grid
                var parentFrame = parentGrid.FindName("view_agregar_producto") as Frame;
                if (parentFrame != null)
                {
                    parentFrame.Content = null;
                   
                    ProductoGuardado?.Invoke(); // Notifica al método padre que la ventana debe cerrarse
                }
                else
                {
                    Debug.WriteLine("No se encontró el Frame 'view_agregar_producto'.");
                }
            }
            else
            {
                Debug.WriteLine("No se pudo encontrar el Grid padre para colapsar.");
            }
        }


        /// <summary>
        /// Maneja el cambio de selección en el ComboBox de método de cálculo.
        /// </summary>
        private void MetodoCalculoRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Llamar a los métodos correspondientes cuando cambia la selección
            MostrarOcultarCampos();
            CalcularPrecioFinal();
        }


        private void MostrarOcultarCampos()
        {
            var selectedRadioButton = MetodoCalculoStackPanel.Children.OfType<RadioButton>()
                .FirstOrDefault(rb => rb.IsChecked == true);

            if (selectedRadioButton != null)
            {
                producto.Eleccion_Calculo = selectedRadioButton.Tag.ToString() == "PrecioFijo";

                if (producto.Eleccion_Calculo)
                {
                    // Mostrar "Precio Fijo" y ocultar "Porcentaje Ganancia"
                    PrecioFijoTextBox.Visibility = Visibility.Visible;
                    PrecioFijoTextBox.IsEnabled = true;

                    PorcentajeGananciaStackPanel.Visibility = Visibility.Collapsed;
                    PorcentajeGananciaTextBox.IsEnabled = false;
                }
                else
                {
                    // Mostrar "Porcentaje Ganancia" y ocultar "Precio Fijo"
                    PorcentajeGananciaStackPanel.Visibility = Visibility.Visible;
                    PorcentajeGananciaTextBox.IsEnabled = true;

                    PrecioFijoTextBox.Visibility = Visibility.Collapsed;
                    PrecioFijoTextBox.IsEnabled = false;
                }
            }
        }


        /// <summary>
        /// Calcula el precio final del producto basado en los datos de entrada.
        /// </summary>
        private void CalcularPrecioFinal()
        {
            try
            {
                // Asignar valores al modelo desde los TextBox
                producto.PrecioCompra = ConvertirTextoAEntero(PrecioCompraTextBox.Text);
                producto.Precio_Fijo = ConvertirTextoADouble(PrecioFijoTextBox.Text);
                producto.Precio_Porcentaje = ConvertirTextoADouble(PorcentajeGananciaTextBox.Text);

                // Determinar el método de cálculo (Precio Fijo o Porcentaje)
                producto.Eleccion_Calculo = MetodoCalculoStackPanel.Children.OfType<RadioButton>()
                                            .FirstOrDefault(rb => rb.IsChecked == true)?.Tag.ToString() == "PrecioFijo";

           

                // Formatear y mostrar el Precio Total en la vista
                var culturaColombiana = new CultureInfo("es-CO")
                {
                    NumberFormat =
            {
                CurrencyDecimalDigits = 0,
                NumberGroupSeparator = "."
            }
                };

                string precioFinalFormateado = producto.PrecioTotal.ToString("N0", culturaColombiana);
                PrecioFinalTextBlock.Text = $" {precioFinalFormateado}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error en CalcularPrecioFinal: {ex.Message}");
            }
        }

        private int ConvertirTextoAEntero(string texto)
        {
            return int.TryParse(texto.Replace(".", "").Replace(",", "").Trim(), out var valor) ? valor : 0;
        }

        private double ConvertirTextoADouble(string texto)
        {
            return double.TryParse(texto.Replace(".", "").Replace(",", "").Trim(), out var valor) ? valor : 0.0;
        }


        private void Actualizar_texto_Precio_Final(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                // Si el texto queda vacío mientras el usuario escribe, colocar "0"
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = "0"; // Restablecer "0" dinámicamente
                    tb.Select(tb.Text.Length, 0); // Evitar que el cursor se mueva
                }
            }

            if (sender is TextBox textBox)
            {
                // Intenta convertir el texto a número
                if (double.TryParse(textBox.Text, out double numero))
                {
                    var culturaColombiana = new CultureInfo("es-CO")
                    {
                        NumberFormat =
                {
                    CurrencyDecimalDigits = 0,
                    NumberGroupSeparator = "."
                }
                    };

                    // Formatea el número con separadores de miles
                    textBox.Text = numero.ToString("N0", culturaColombiana);

                    // Establece el cursor al final del texto para evitar comportamiento extraño
                    textBox.SelectionStart = textBox.Text.Length;
                }
            }
            CalcularPrecioFinal();
        }
    
        private void ActualizarValorPorcentaje(int incremento)
        {

            // Eliminar el símbolo "%" del texto para realizar la conversión numérica
            string textoSinPorcentaje = PorcentajeGananciaTextBox.Text.TrimEnd('%');

            // Verificar si el contenido del TextBox es un número válido
            if (int.TryParse(textoSinPorcentaje, out int valorActual))
            {
                // Actualizar el valor sumando o restando
                int nuevoValor = valorActual + incremento;

                // Asegurarte de que el valor no sea negativo (opcional)
                if (nuevoValor < 0)
                {
                    nuevoValor = 0;
                }

                // Actualizar el TextBox con el nuevo valor
                PorcentajeGananciaTextBox.Text = $"{nuevoValor}";

                // Posicionar el cursor antes del símbolo "%"
                PorcentajeGananciaTextBox.SelectionStart = PorcentajeGananciaTextBox.Text.Length - 1;
            }
            else
            {
                // Si el TextBox no tiene un valor válido, inicializarlo con 0%
                PorcentajeGananciaTextBox.Text = "0%";

                // Posicionar el cursor antes del símbolo "%"
                PorcentajeGananciaTextBox.SelectionStart = PorcentajeGananciaTextBox.Text.Length - 1;
            }
        }

        private void BotonIzquierdo_Click(object sender, RoutedEventArgs e)
        {
            // Llama al método para restar 5
            ActualizarValorPorcentaje(-5);
        }

        private void BotonDerecho_Click(object sender, RoutedEventArgs e)
        {
            // Llama al método para sumar 5
            ActualizarValorPorcentaje(5);
        }


        #region creacion de referencias
        /// <summary>
        /// Lógica para la implementación de referencia
        /// </summary>

       

        #endregion


        #region reglas entrada y salida texto

        private bool _debeLimpiarCero = false;

            private void TextBox_GotFocus(object sender, RoutedEventArgs e)
            {
                if (sender is TextBox tb)
                {
                    if (tb.Text == "0")
                    {
                        _debeLimpiarCero = true; // Activar la lógica para limpiar el "0"
                    }
                }
            }

            private void TextBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
            {
                if (sender is TextBox tb)
                {
                    if (_debeLimpiarCero && tb.Text == "0")
                    {
                        tb.Text = ""; // Limpia el contenido
                        _debeLimpiarCero = false; // Reinicia el indicador
                    }
                }
            }

            private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
            {
                if (sender is TextBox tb)
                {
                    // Si el texto queda vacío mientras el usuario escribe, colocar "0"
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        tb.Text = "0"; // Restablecer "0" dinámicamente
                        tb.Select(tb.Text.Length, 0); // Evitar que el cursor se mueva
                    }
                }
            }

            private void TextBox_LostFocus(object sender, RoutedEventArgs e)
            {
                if (sender is TextBox tb)
                {
                    // Si el contenido está vacío, mantener el "0" al perder foco
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        tb.Text = "0";
                    }
                }
            }

        #endregion

     


    }
}

