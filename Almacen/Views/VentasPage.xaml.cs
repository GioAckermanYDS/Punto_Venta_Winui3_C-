using Almacen.Models;
using Almacen.Estilos_Configuracion.Estilos;
using Almacen.Views.View_Almacen;
using AlmacenApp.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Microsoft.UI;
using System.ComponentModel;

namespace AlmacenApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VentasPage : Page
    {

        #region        
        public ColorDeFondo Tema => ColorDeFondo.Instancia;


        #endregion

        #region Constructor

        public VentasPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
           



        }






        #endregion

        #region Btn_App

        private async void RealizarVenta_Click(object sender, RoutedEventArgs e)
        {
            // 🔥 Mostrar mensaje de confirmación antes de realizar la venta
            var confirmacionDialog = new ContentDialog
            {
                Title = "Confirmar venta",
                Content = "¿Estás seguro de que deseas realizar la venta? Esta acción actualizará el stock de los productos.",
                PrimaryButtonText = "Aceptar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.XamlRoot, // Siempre necesario en WinUI 3
                Background = Tema.FondoPrimario,
                Foreground = Tema.ColorDeLetra,
                BorderBrush = Tema.FondoTersario,
                BorderThickness = new Thickness(3)
            };


            var resultado = await confirmacionDialog.ShowAsync();

            // 🚀 Si el usuario presiona "Aceptar", proceder con la venta
            if (resultado == ContentDialogResult.Primary)
            {
                foreach (var p in productosSeleccionados)
                {
                    // 🔥 Obtener la cantidad vendida
                    int cantidadVendida = cantidadesVenta.ContainsKey(p.ID) ? cantidadesVenta[p.ID] : 0;

                    // 🔥 Verificar que haya stock suficiente antes de actualizar
                    if (cantidadVendida > p.Cantidad)
                    {
                        await MostrarMensajeErrorStock(p.Nombre);
                        return;
                    }

                    // 🔥 Restar la cantidad vendida al stock usando el nuevo método RealizarVenta()
                    await Bd_Producto.RealizarVenta(p.ID, p.Cantidad - cantidadVendida);
                }

                // ✅ Limpiar la lista después de la venta
                productosSeleccionados.Clear();
                cantidadesVenta.Clear();
                ListaResumenVenta.ItemsSource = null;
                ListaResumenVenta.ItemsSource = new List<object>(); // 🔥 Vaciar lista de venta

                // ✅ Actualizar la lista de productos para reflejar el nuevo stock
                await CargarProductosAsync();

                // ✅ Mostrar mensaje de venta exitosa
                await MostrarMensajeVentaExitosa();
            }
        }





        // 🔥 Mensaje de error si el stock es insuficiente
        private async Task MostrarMensajeErrorStock(string productoNombre)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Stock insuficiente",
                Content = $"No hay suficiente stock disponible de {productoNombre} para realizar la venta.",
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot, // Siempre necesario en WinUI 3
                Background = Tema.FondoPrimario,
                Foreground = Tema.ColorDeLetra,
                BorderBrush = Tema.FondoTersario,
                BorderThickness = new Thickness(3)

            };

            await errorDialog.ShowAsync();
        }

        // 🔥 Mensaje de confirmación cuando la venta se realiza correctamente
        private async Task MostrarMensajeVentaExitosa()
        {
            ContentDialog exitoDialog = new ContentDialog
            {
                Title = "Venta realizada",
                Content = "La venta se completó correctamente",
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot, // Siempre necesario en WinUI 3
                Background = Tema.FondoPrimario,
                Foreground = Tema.ColorDeLetra,
                BorderBrush = Tema.FondoTersario,
                BorderThickness = new Thickness(3)

            };

            await exitoDialog.ShowAsync();
        }


        private async void CancelarVenta_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog confirmacionDialog = new ContentDialog
            {
                Title = "Confirmar cancelación",
                Content = "¿Estás seguro de que deseas cancelar la venta? Esta acción eliminará todos los productos seleccionados.",
                PrimaryButtonText = "Aceptar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.XamlRoot,
                Background = Tema.FondoPrimario,
                Foreground = Tema.ColorDeLetra,
                BorderBrush = Tema.FondoTersario,
                BorderThickness = new Thickness(3)


            };

            var resultado = await confirmacionDialog.ShowAsync();

            // 🚀 Si el usuario confirma, limpiar la lista
            if (resultado == ContentDialogResult.Primary)
            {
                productosSeleccionados.Clear();
                cantidadesVenta.Clear();
                ListaResumenVenta.ItemsSource = null;
                ListaResumenVenta.ItemsSource = new List<object>(); // ✅ Vaciar lista

                // 🔥 Reiniciar total de venta
                TotalVentaTextBlock.Text = "0";

                Debug.WriteLine("===== VENTA CANCELADA =====");
                Debug.WriteLine("ListaResumenVenta ahora está vacía.");
            }
        }



        #endregion


        #region lista de productos
        private List<Producto> productosOriginales = new List<Producto>();

        public async Task CargarProductosAsync()
        {
            var productosPorCategoria = await Bd_Producto.GetTodosLosProductosPorCategoriaAsync();
            productosOriginales = productosPorCategoria.SelectMany(kvp => kvp.Value).ToList();

            // Mostrar inicialmente todos los productos
            ListaProductosVentas.ItemsSource = productosOriginales;
        }

        // Evento para filtrar productos
        private void FiltroProductoVenta_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filtro = FiltroProductoVenta.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(filtro))
            {
                ListaProductosVentas.ItemsSource = productosOriginales; // Restaurar la lista completa
            }
            else
            {
                var productosFiltrados = productosOriginales
                    .Where(p => (p.Nombre ?? "").ToLower().Contains(filtro) ||
                                (p.Descripcion ?? "").ToLower().Contains(filtro))
                    .ToList();

                ListaProductosVentas.ItemsSource = productosFiltrados;
            }
        }




        // Llamar la carga de datos al navegar a la página
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await CargarProductosAsync();
        }


        private void ListaProductosVentas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(ListaProductosVentas);
            if (scrollViewer != null)
            {
                scrollViewer.ChangeView(null, scrollViewer.VerticalOffset + 50, null); // Ajusta el desplazamiento
            }
        }

        private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return default; // Verifica que el objeto raíz no sea nulo

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }

            return default; // Devuelve `default(T)?` en lugar de `null`
        }


        #endregion

        #region Resumen de venta lista

        private List<Producto> productosSeleccionados = new List<Producto>();

        private Dictionary<int, int> cantidadesVenta = new Dictionary<int, int>();


        private async void ListaProductosVentas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var productoSeleccionado = (sender as ListView)?.SelectedItem as Producto;
            if (productoSeleccionado != null)
            {
                // 🔍 Obtener la cantidad actual en la lista de venta
                int cantidadActual = cantidadesVenta.ContainsKey(productoSeleccionado.ID) ? cantidadesVenta[productoSeleccionado.ID] : 0;

                // 🔥 Verificar si la cantidad excede el stock disponible
                if (cantidadActual >= productoSeleccionado.Cantidad)
                {
                    await MostrarMensajeSinStock(); // 🚫 Mostrar alerta si ya no hay más stock
                    return;
                }

                // ✅ Si hay stock disponible, agregar el producto
                if (cantidadesVenta.ContainsKey(productoSeleccionado.ID))
                {
                    cantidadesVenta[productoSeleccionado.ID]++;
                }
                else
                {
                    cantidadesVenta[productoSeleccionado.ID] = 1;
                    productosSeleccionados.Add(productoSeleccionado);
                }

                // Actualizar la lista
                ListaResumenVenta.ItemsSource = null;
                ListaResumenVenta.ItemsSource = productosSeleccionados.Select(p => new
                {
                    p.ID,
                    p.Nombre,
                    CantidadVenta = cantidadesVenta.ContainsKey(p.ID) ? cantidadesVenta[p.ID] : 0,
                    Total = cantidadesVenta.ContainsKey(p.ID) ? cantidadesVenta[p.ID] * p.Total : 0
                }).ToList();

                ActualizarTotalVenta();
            }
        }

        // 🔥 Método para mostrar el mensaje de "No hay más productos en el almacén"
        private async Task MostrarMensajeSinStock()
        {
            ContentDialog noStockDialog = new ContentDialog
            {
                Title = "Stock insuficiente",
                Content = "No se puede agregar más unidades de este producto. No hay más en el almacén.",
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot // ✅ Asignar el XamlRoot de la página actual
            };

            await noStockDialog.ShowAsync();
        }

        private void ListaResumenVenta_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var itemSeleccionado = (sender as ListView)?.SelectedItem;
            if (itemSeleccionado != null)
            {
                var propertyInfo = itemSeleccionado.GetType().GetProperty("ID");
                var productoId = propertyInfo != null ? (propertyInfo.GetValue(itemSeleccionado) as int?) ?? -1 : -1;
                if (productoId == -1) return;

                if (cantidadesVenta.ContainsKey(productoId))
                {
                    if (cantidadesVenta[productoId] > 1)
                    {
                        cantidadesVenta[productoId]--;
                    }
                    else
                    {
                        cantidadesVenta.Remove(productoId);
                        productosSeleccionados.RemoveAll(p => p.ID == productoId);
                    }
                }

                ListaResumenVenta.ItemsSource = null;
                ListaResumenVenta.ItemsSource = productosSeleccionados.Select(p => new
                {
                    p.ID,
                    p.Nombre,
                    CantidadVenta = cantidadesVenta.ContainsKey(p.ID) ? cantidadesVenta[p.ID] : 0,
                    Total = cantidadesVenta.ContainsKey(p.ID) ? cantidadesVenta[p.ID] * p.Total : 0 // ✅ Corrección del cálculo
                }).ToList();


                ActualizarTotalVenta();

                
            }
        }

        #region Operacion para suma de productos
        private void ActualizarTotalVenta()
        {
            int totalVenta = 0;

            Debug.WriteLine("===== ACTUALIZANDO TOTAL DE VENTA =====");

            foreach (var p in productosSeleccionados)
            {
                // 🔥 Obtener la cantidad correcta del producto
                int cantidad = cantidadesVenta.ContainsKey(p.ID) ? cantidadesVenta[p.ID] : 0;

                // 🔥 Calcular correctamente el total de cada producto
                int totalPorProducto = (int)(cantidad * p.Total); // 🔥 Conversión explícita de `double` a `int`


                Debug.WriteLine($"Producto ID: {p.ID}, Nombre: {p.Nombre}, Cantidad: {cantidad}, Total Correcto: {totalPorProducto}");

                totalVenta += totalPorProducto; // 🔥 Sumar al total de la venta general
            }

            // 🔥 Actualizar el total de la venta
            TotalVentaTextBlock.Text = totalVenta.ToString("N0");

            Debug.WriteLine($"TOTAL FINAL DE VENTA: {totalVenta}");
        }

        #endregion
        #endregion

        #region filtro de mayor a menor 

        private Dictionary<string, int> estadoOrden = new Dictionary<string, int>
{
            { "Nombre", 0 },
              { "Descripcion", 0 },
    { "Cantidad", 0 },
    { "Total", 0 }
};



        private void OrdenarLista(object sender, RoutedEventArgs e)
        {
            if (ListaProductosVentas.ItemsSource is List<Producto> productos && productosOriginales != null)
            {
                var textBlock = sender as TextBlock;
                if (textBlock == null) return;

                string propiedad = textBlock.Text.Replace(" ↓", "").Replace(" ↑", "").Trim(); // Limpiar texto dinámico

                // Resetear símbolos en todos los encabezados excepto el actual
                ResetearSimbolos(propiedad);

                // Definir la propiedad de orden según el TextBlock presionado
                Func<Producto, object>? selector = propiedad switch
                {
                    "Nombre" => p => p.Nombre,
                    "Descripcion" => p => p.Descripcion,
                    "Cantidad" => p => p.Cantidad,
                    "Total" => p => p.PrecioTotal,
                    _ => null
                };

                if (selector == null) return;

                // Alternar estado de orden entre 0 (original), 1 (ascendente ↓) y 2 (descendente ↑)
                estadoOrden[propiedad] = (estadoOrden[propiedad] + 1) % 3;

                if (estadoOrden[propiedad] == 1)
                {
                    productos = productos.OrderBy(selector).ToList(); // Ascendente (A-Z)
                    textBlock.Text = $"{propiedad} ↓";
                }
                else if (estadoOrden[propiedad] == 2)
                {
                    productos = productos.OrderByDescending(selector).ToList(); // Descendente (Z-A)
                    textBlock.Text = $"{propiedad} ↑";
                }
                else
                {
                    productos = new List<Producto>(productosOriginales); // Restaurar lista
                    textBlock.Text = propiedad;
                }

                // Refrescar la lista visualmente
                ListaProductosVentas.ItemsSource = null;
                ListaProductosVentas.ItemsSource = productos;
            }
        }


        // Método para resetear los símbolos en los encabezados excepto el actual
        private void ResetearSimbolos(string propiedadActiva)
        {
            foreach (var clave in estadoOrden.Keys)
            {
                if (clave != propiedadActiva)
                {
                    estadoOrden[clave] = 0; // Resetear estado
                    var textBlock = FindName(clave) as TextBlock; // Buscar el TextBlock por nombre
                    if (textBlock != null)
                    {
                        textBlock.Text = clave; // Restaurar el nombre sin símbolos
                    }
                }
            }
        }

        #endregion

    }
}
