using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Diagnostics;
using Almacen.Models;
using AlmacenApp.Data;
using Windows.System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Globalization;
using Almacen.Data;
using System.Collections.Generic;
using Almacen.Estilos_Configuracion.Estilos;




namespace Almacen.Views.View_Almacen
{
    /// <summary>
    /// Clase View_Productos que gestiona la vista de productos.
    /// </summary>
    public sealed partial class View_Productos : Page
    {
        #region Variables globales
        public event Action? ViewProductosCerrada;
        private Producto producto = new Producto();
        private int CategoriaId { get; set; }
        private int productoId;      
        private ViewCategorias parentViewCategorias;
        public ColorDeFondo Tema => ColorDeFondo.Instancia;
        public Producto ProductoActual { get; set; } = new Producto();


        #endregion


        #region Constructor
        /// <summary>
        /// Constructor de la clase View_Productos.
        /// </summary>
        /// <param name="productoId">El ID del producto que se está gestionando.</param>
        public View_Productos(int productoId, ViewCategorias parentView)
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.productoId = productoId;
            this.parentViewCategorias = parentView;

            ProductoActual = new Producto(); // o un producto existente
            this.DataContext = ProductoActual; // ← clave

            this.Loaded += async (s, e) =>
            {
                await InicializarProductoAsync(productoId);
                Carga_De_datos(); // Ahora sí, el DataContext ya estará listo
            };
        }


        private void Carga_De_datos()
        {
            var vm = (Producto)this.DataContext;
            NombreTextBox.Text = vm.Nombre.ToString();
            CantidadTextBox.Text = vm.Cantidad.ToString();
            PrecioCompraTextBox.Text = vm.PrecioCompra.ToString();
            DescripcionTextBox.Text = vm.Descripcion.ToString();
            PrecioFijoTextBox.Text = vm.Precio_Fijo.ToString();
            PorcentajeGananciaTextBox.Text = vm.Precio_Porcentaje.ToString();
        }

        private async Task InicializarProductoAsync(int productoId)
        {
            
           
            try
            {               

                // Llama al método para obtener el producto por su ID
                var productos = await Bd_Producto.GetProductosPorCategoriaAsync(0, productoId);

                // Verifica si se encontró el producto
                if (productos.Any())
                {
                    this.producto = productos.First(); // Extrae el primer producto (debería haber solo uno)

                    // Establece el DataContext inmediatamente después de asignar el producto
                    this.DataContext = this.producto;
                    

                    // Configura los RadioButton según el valor de UsarPrecioFijo
                    PrecioFijoRadioButton.IsChecked = producto.Eleccion_Calculo;
                    PorcentajeGananciaRadioButton.IsChecked = !producto.Eleccion_Calculo;

                    // Ajuste para cargar el porcentaje como número entero
                    if (!producto.Eleccion_Calculo)
                    {
                        int porcentajeEntero = (int)Math.Round(producto.Precio_Porcentaje); // Redondea y convierte a entero
                        PorcentajeGananciaTextBox.Text = porcentajeEntero.ToString(); // Asigna como entero                  
                    }
                    else
                    {
                        // Configuración para Precio Fijo (ya funciona perfectamente)
                        PrecioFijoTextBox.Text = producto.Precio_Fijo.ToString("F2");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al inicializar producto: {ex.Message}");
            }
        }

      

        #endregion


        #region Métodos para botones


        private async void Btn_Modificar(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener valores desde la interfaz
                int productoId = this.productoId;
                string nuevoNombre = NombreTextBox.Text;
                string nuevaDescripcion = DescripcionTextBox.Text;
                decimal precioCompra = decimal.Parse(PrecioCompraTextBox.Text.Replace(".", "").Trim());
                decimal precioFijo = 0; // Valor por defecto
                decimal porcentajeGanancia = 0; // Valor por defecto
                int cantidad = int.Parse(CantidadTextBox.Text.Trim());

                // Determinar el método de cálculo seleccionado y asignar valores
                bool eleccionCalculoBool = PrecioFijoRadioButton.IsChecked == true; // true para PrecioFijo, false para PorcentajeGanancia
                string eleccionCalculo = eleccionCalculoBool ? "1" : "0"; // Convertir el booleano a texto ("1" o "0")

                if (eleccionCalculoBool) // Si la elección es PrecioFijo
                {
                    precioFijo = decimal.Parse(PrecioFijoTextBox.Text.Replace(".", "").Trim());
                    porcentajeGanancia = 0; // Formatear porcentaje como vacío (o cero)
                }
                else // Si la elección es PorcentajeGanancia
                {
                    porcentajeGanancia = decimal.Parse(PorcentajeGananciaTextBox.Text.Replace(".", "").Trim());
                    precioFijo = 0; // Formatear precio fijo como vacío (o cero)
                }

                // Validar que el productoId sea válido
                if (productoId <= 0)
                {
                    Debug.WriteLine("Por favor, seleccione un producto válido.");
                    return;
                }

                // Modificar el producto en la base de datos
                int resultado = await Bd_Producto.Modificar_Producto(
                    productoId,
                    nuevoNombre,
                    nuevaDescripcion,
                    precioCompra,
                    precioFijo, // Guardar precio fijo o cero
                    eleccionCalculo, // Guardar "1" o "0" según la elección
                    porcentajeGanancia, // Guardar porcentaje o cero
                    cantidad
                );

                // Manejar el resultado
                if (resultado > 0)
                {
                    Debug.WriteLine("El producto fue modificado exitosamente.");

                    // Refrescar la vista principal
                    if (parentViewCategorias != null)
                    {
                        await parentViewCategorias.CargarProductosAsync();
                    }
                    else
                    {
                        Debug.WriteLine("No se pudo acceder a ViewCategorias para refrescar los productos.");
                    }

                    ViewProductosCerrada?.Invoke();

                    // Colapsar el Grid padre y limpiar contenido
                    var parent = this.Parent as FrameworkElement;
                    while (parent != null && !(parent is Grid))
                    {
                        parent = parent.Parent as FrameworkElement;
                    }

                    if (parent is Grid parentGrid)
                    {
                        parentGrid.Visibility = Visibility.Collapsed;

                        var parentFrame = parentGrid.FindName("view_agregar_producto") as Frame;
                        if (parentFrame != null)
                        {
                            parentFrame.Content = null;
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
                else
                {
                    Debug.WriteLine("Error al modificar el producto. Verifique el ID.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ocurrió un error al intentar modificar el producto: {ex.Message}");
            }
        }

        private async void Btn_Eliminar(object sender, RoutedEventArgs e)
        {
            try
            {
                int productoId = this.productoId;

                if (productoId <= 0)
                {
                    Debug.WriteLine("Por favor, seleccione un producto válido.");
                    return;
                }

                var dialogo = new ContentDialog
                {
                    Title = "Confirmar eliminación",
                    Content = $"¿Seguro que deseas eliminar el producto? Esta acción no se puede deshacer.",
                    PrimaryButtonText = "Eliminar",
                    CloseButtonText = "Cancelar",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot,
                    Background = Tema.FondoPrimario,
                    Foreground = Tema.ColorDeLetra,
                    BorderBrush = Tema.FondoTersario,
                    BorderThickness = new Thickness(3)
                };

                var resultado = await dialogo.ShowAsync();

                if (resultado == ContentDialogResult.Primary)
                {                  

                    // Luego, eliminar el producto
                    int filasEliminadas = await Bd_Producto.EliminarProducto(productoId);

                    if (filasEliminadas > 0)
                    {
                        Debug.WriteLine("El producto fue eliminado exitosamente.");

                       

                        ViewProductosCerrada?.Invoke();
                    }
                    else
                    {
                        Debug.WriteLine("Error al eliminar el producto. Verifique el ID.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ocurrió un error al intentar eliminar el producto: {ex.Message}");
            }
        }






        private void Btn_Cerrar(object sender, RoutedEventArgs e)
        {
            // Disparar el evento para notificar que View_Productos se ha cerrado
            ViewProductosCerrada?.Invoke();

            // Buscar el Grid padre y colapsarlo
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


        #endregion

        #region Métodos relacionados con la lógica de la vista
        /// <summary>
        /// Actualiza el texto del precio final.
        /// Este método se ejecuta cuando cambia el texto de un control de entrada relacionado.
        /// </summary>
        private void Actualizar_texto_Precio_Final(object sender, TextChangedEventArgs e)
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

            var textBox = sender as TextBox;
            if (textBox == null) return;

            // Guardar posición actual del cursor
            int originalCursor = textBox.SelectionStart;

            // Quitar puntos para analizar el número sin formato
            string rawText = textBox.Text.Replace(".", "");

            // Calcular cuántos dígitos hay antes del cursor
            int digitsBeforeCursor = 0;
            for (int i = 0; i < originalCursor && i < textBox.Text.Length; i++)
            {
                if (char.IsDigit(textBox.Text[i]))
                    digitsBeforeCursor++;
            }

            // Tratar de convertir a número
            if (decimal.TryParse(rawText, out decimal valor))
            {
                var culturaColombiana = new CultureInfo("es-CO")
                {
                    NumberFormat =
            {
                CurrencyDecimalDigits = 0,
                NumberGroupSeparator = "."
            }
                };

                string formateado = valor.ToString("N0", culturaColombiana);

                if (textBox.Text != formateado)
                {
                    textBox.Text = formateado;

                    // Calcular nueva posición del cursor en base a la cantidad de dígitos antes del cursor
                    int newCursor = 0;
                    int digitCount = 0;
                    while (newCursor < formateado.Length && digitCount < digitsBeforeCursor)
                    {
                        if (char.IsDigit(formateado[newCursor]))
                            digitCount++;
                        newCursor++;
                    }

                    textBox.SelectionStart = newCursor;
                }
            }

            CalcularPrecioFinal();
        }





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

        #region Métodos para eventos adicionales
        /// <summary>
        /// Maneja el evento de clic del botón derecho.
        /// </summary>
        private void BotonDerecho_Click(object sender, RoutedEventArgs e)
        {
            ActualizarValorPorcentaje(5);
        }

        /// <summary>
        /// Maneja el evento de clic del botón izquierdo.
        /// </summary>
        private void BotonIzquierdo_Click(object sender, RoutedEventArgs e)
        {
            ActualizarValorPorcentaje(-5);
        }
        
        #endregion

        #region Ajustes (Configuraciones) Y metodos logicos      
        private void CalcularPrecioFinal()
        {
            try
            {
                // Validar y asignar PrecioCompra
                if (!string.IsNullOrWhiteSpace(PrecioCompraTextBox.Text))
                {
                    producto.PrecioCompra = int.TryParse(PrecioCompraTextBox.Text.Replace(".", "").Replace(",", "").Trim(), out var pc) ? pc : producto.PrecioCompra;
                }

                // Validar y asignar Precio_Fijo
                producto.Precio_Fijo = double.TryParse(PrecioFijoTextBox.Text.Replace(".", "").Replace(",", "").Trim(), out var pf) ? pf : producto.Precio_Fijo;

                // Validar y asignar Precio_Porcentaje
                producto.Precio_Porcentaje = double.TryParse(PorcentajeGananciaTextBox.Text.Replace(".", "").Replace(",", "").Trim(), out var pg) ? pg : producto.Precio_Porcentaje;

                // Determinar el método de cálculo
                producto.Eleccion_Calculo = MetodoCalculoStackPanel.Children.OfType<RadioButton>()
                                            .FirstOrDefault(rb => rb.IsChecked == true)?.Tag.ToString() == "PrecioFijo";

                // Formatear PrecioTotal para el usuario
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
        private void MetodoCalculoRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                if (radioButton.Tag?.ToString() == "PrecioFijo")
                {
                    producto.Eleccion_Calculo = true;

                    // Mostrar "Precio Fijo" y ocultar "Porcentaje Ganancia"
                    PrecioFijoTextBox.Visibility = Visibility.Visible;
                    PrecioFijoTextBox.IsEnabled = true;

                    PorcentajeGananciaStackPanel.Visibility = Visibility.Collapsed;
                    PorcentajeGananciaTextBox.IsEnabled = false;

                }
                else if (radioButton.Tag?.ToString() == "PorcentajeGanancia")
                {
                    producto.Eleccion_Calculo = false;

                    // Mostrar "Porcentaje Ganancia" y ocultar "Precio Fijo"
                    PorcentajeGananciaStackPanel.Visibility = Visibility.Visible;
                    PorcentajeGananciaTextBox.IsEnabled = true;

                    PrecioFijoTextBox.Visibility = Visibility.Collapsed;
                    PrecioFijoTextBox.IsEnabled = false;
                }
            }

            

            // Recalcular el precio final
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


        #endregion


    }
}
