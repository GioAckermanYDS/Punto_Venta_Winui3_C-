using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Almacen.Models;
using AlmacenApp.Data;
using AlmacenApp.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Windows.Storage.Pickers;
using Windows.Storage;
using AlmacenApp.Views;
using System.Threading.Tasks;
using System.Windows;
using Almacen.Views.View_Almacen;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;
using Windows.UI;

using System.Globalization;
using Almacen.Style.Efectos;
using Almacen.Estilos_Configuracion.Estilos;
using Microsoft.UI;




namespace Almacen.Views.View_Almacen
{
    public sealed partial class ViewCategorias : Page
    {
        #region Variables globales

       

        public event Action? ViewCategoriasCerrada;

        public string NombreCategoria { get; set; }
        private int CategoriaId { get; set; }
        public string? ImagenUrl { get; set; }

       
        private int productoId;

        private ObservableCollection<Categoria> listaCategorias;
        private RefrescarData refrescarData;
        public ColorDeFondo Tema => ColorDeFondo.Instancia;

        #endregion

        #region Constructor


        public ViewCategorias(int categoriaId, string nombreCategoria, BitmapImage imagenPreview, ObservableCollection<Categoria> listaCategorias, ItemsRepeater categoriasRepeater)
        {
           
            this.InitializeComponent();

            this.DataContext = RootPage;

            ProductosListView.SelectionChanged += Seleccionar_Producto_List;

            this.listaCategorias = listaCategorias;

               
            var viewProductos = new View_Productos(productoId, this);

            CategoriaId = categoriaId;
            NombreCategoria = nombreCategoria;

            if (NombreTextBlock != null)
            {
                NombreTextBlock.Text = nombreCategoria;
            }

            try
            {
                var imageBrush = new ImageBrush
                {
                    ImageSource = imagenPreview ?? new BitmapImage(new Uri("ms-appx:///Assets/default.png"))
                };
                CategoriaImage.Fill = imageBrush;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error cargando imagen: {ex.Message}");
            }

            this.refrescarData = new RefrescarData(listaCategorias, new ObservableCollection<Producto>(), categoriasRepeater);
            _ = InicializarDatosAsync(categoriaId);


           

        }



        #endregion

        #region Inicialización de Datos

        private async Task InicializarDatosAsync(int categoriaId)
        {        
            try
            {
                await CargarProductosAsync();

                productoId = -1;

               

                List<Producto> productos = await Bd_Producto.GetProductosPorCategoriaAsync(categoriaId);

                // Asignar la lista de productos al ListView
                ProductosListView.ItemsSource = productos;
            }
            catch (SqliteException ex)
            {
                Debug.WriteLine($"❌ SqliteException - InicializarDatosAsync - View_Categorias : {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error - InicializarDatosAsync - View_Categorias : {ex.Message}");
            }
        }

       

        // Desuscripción para evitar fugas de memoria
       

        #endregion

        #region Botones de la pagina

        private async void Btn_Regresar(object sender, RoutedEventArgs e)
        {
            try
            {
                await refrescarData.RefrescarCategoriasAsync();
                ViewCategoriasCerrada?.Invoke();

                if (this.Parent is Frame frame && frame.Parent is Grid grid)
                {
                    grid.Visibility = Visibility.Collapsed;
                    frame.Content = null;
                   
                }
                else
                {
                    Debug.WriteLine("⚠️ No se pudo cerrar correctamente la vista. Verifica la jerarquía de controles.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al intentar cerrar la vista - Btn_Regresar: {ex.Message}");
            }
        }

        private async void Btn_Modificar_Categoria(object sender, RoutedEventArgs e)
        {
            // Obtener la categoría desde la base de datos
            Categoria? categoria = await Bd_Categoria.ObtenerCategoriaPorIdAsync(CategoriaId);

            if (categoria == null)
            {
                Debug.WriteLine("[ERROR] La categoría no existe.");
                return;
            }

            // Crear controles del diálogo
            Button seleccionarImagenButton = new Button { Content = "Seleccionar Imagen", HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(10) };
            Image imagenPreview = new Image { Width = 150, Height = 150, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(10) };
            TextBox nombreTextBox = new TextBox { PlaceholderText = "Nuevo nombre", Text = categoria.Nombre, Margin = new Thickness(10) };

            // Mostrar imagen actual desde la base de datos
            BitmapImage bitmapImage = new BitmapImage();
            if (categoria.Imagen != null && categoria.Imagen.Length > 0)
            {
                using var stream = new MemoryStream(categoria.Imagen);
                await bitmapImage.SetSourceAsync(stream.AsRandomAccessStream());
                imagenPreview.Source = bitmapImage;
            }
            else
            {
                imagenPreview.Source = new BitmapImage(new Uri("ms-appx:///Assets/default.png"));
            }

            // Lógica para seleccionar nueva imagen
            seleccionarImagenButton.Click += async (s, args) =>
            {
                var picker = new FileOpenPicker();
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".jpeg");

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow ?? throw new InvalidOperationException("MainWindow no inicializada"));
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                StorageFile file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    using var stream = await file.OpenStreamForReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    categoria.Imagen = memoryStream.ToArray();

                    using var fileStream = await file.OpenAsync(FileAccessMode.Read);
                    await bitmapImage.SetSourceAsync(fileStream);
                    imagenPreview.Source = bitmapImage;

                    seleccionarImagenButton.Content = "Imagen seleccionada";
                }
            };

            // Crear el contenido del diálogo
            StackPanel dialogStackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            dialogStackPanel.Children.Add(seleccionarImagenButton);
            dialogStackPanel.Children.Add(imagenPreview);
            dialogStackPanel.Children.Add(nombreTextBox);

            var editarCategoriaDialog = new ContentDialog
            {
                Title = "Editar categoría",
                Content = dialogStackPanel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.XamlRoot,
                Background = Tema.FondoPrimario,
                Foreground = Tema.ColorDeLetra,
                BorderBrush = Tema.FondoTersario,
                BorderThickness = new Thickness(3)
            };

            ContentDialogResult result = await editarCategoriaDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string nuevoNombre = nombreTextBox.Text;

                await Bd_Categoria.Modificar_Categoria(categoria.Id, categoria.Imagen ?? Array.Empty<byte>(), nuevoNombre);

                NombreCategoria = nuevoNombre;
                NombreTextBlock.Text = nuevoNombre;

                using var stream = new MemoryStream(categoria.Imagen ?? Array.Empty<byte>());
                await bitmapImage.SetSourceAsync(stream.AsRandomAccessStream());
                CategoriaImage.Fill = new ImageBrush { ImageSource = bitmapImage };

                await refrescarData.RefrescarCategoriasAsync();
            }
        }




        private async void Btn_Borrar_Categoria(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Eliminar categoría",
                Content = "¿Estás seguro de que deseas eliminar esta categoría?",
                PrimaryButtonText = "Eliminar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.Content.XamlRoot,
                Background = Tema.FondoPrimario,
                Foreground = Tema.ColorDeLetra,
                BorderBrush = Tema.FondoTersario,
                BorderThickness = new Thickness(3)
            };

            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    // Eliminar categoría de la base de datos
                    await Bd_Categoria.EliminarCategoriaAsync(CategoriaId);

                    // Cerrar la vista antes de refrescar
                    Btn_Regresar(sender, e);

                    // Asegurar que la lista de categorías se refresca correctamente
                    await refrescarData.RefrescarCategoriasAsync();

                    
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"🚨 [ERROR] Btn_Borrar_Categoria - View_Categorias: {ex.Message}");
                }
            }
        }


        private void Btn_Agregar_Producto(object sender, RoutedEventArgs e)
        {
            try
            {
                Fondo_Sombra.ActivarSombra(ShadowLayer, customHeight: 440, margin: new Thickness(30, 60, 30, 0));
                

                ShadowLayer.Visibility = Visibility.Visible;
                main_categoria.Visibility = Visibility.Collapsed;
                main_agregar_producto.Visibility = Visibility.Visible;

                var viewAgregarProductoPage = new View_Agregar_Producto(CategoriaId);



                // Evento al guardar producto
                viewAgregarProductoPage.ProductoGuardado += async () =>
                {
                    Fondo_Sombra.DesactivarSombra(ShadowLayer);
                    ShadowLayer.Visibility = Visibility.Collapsed;
                    main_agregar_producto.Visibility = Visibility.Collapsed;
                    MainContent.Visibility = Visibility.Visible;
                    await CargarProductosAsync();
                };

                // Asignar la vista al contenedor
                view_agregar_producto.Content = viewAgregarProductoPage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en Btn_Agregar_Producto: {ex.Message}");
            }
        }

        private void Seleccionar_Producto_List(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Fondo_Sombra.ActivarSombra(ShadowLayer, customHeight: 440, margin: new Thickness(30, 60, 30, 0));
                ShadowLayer.Visibility = Visibility.Visible;

                if (e.AddedItems.Count > 0)
                {
                    var productoSeleccionado = e.AddedItems[0] as Producto;
                    if (productoSeleccionado != null)
                    {
                        MainContent.Visibility = Visibility.Collapsed;
                        main_categoria.Visibility = Visibility.Visible;

                        var viewProductosPage = new View_Productos(productoSeleccionado.ID, this);

                        // Asignar la vista al contenedor
                        view_productos.Content = viewProductosPage;

                        // Evento para cerrar vista detalle
                        viewProductosPage.ViewProductosCerrada += async () =>
                        {
                            MainContent.Visibility = Visibility.Visible;
                            main_categoria.Visibility = Visibility.Collapsed;
                            ProductosListView.SelectedItem = null;

                            Fondo_Sombra.DesactivarSombra(ShadowLayer);
                            ShadowLayer.Visibility = Visibility.Collapsed;

                            await CargarProductosAsync();
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en Seleccionar_Producto_List - View_Categorias: {ex.Message}");
            }
        }

        #endregion

        #region Cargar Productos



        // Variable global para almacenar la lista completa
        private List<Producto> productosOriginales = new List<Producto>();

        #region Cargar Productos
        public async Task CargarProductosAsync()
        {
            try
            {
                Debug.WriteLine($"✅ Se cargaron {productosOriginales.Count} productos para la categoría {CategoriaId}");
                // Cargar productos desde la base de datos
                productosOriginales = await Bd_Producto.GetProductosPorCategoriaAsync(CategoriaId);

                await refrescarData.RefrescarProductosAsync(CategoriaId);
                ProductosListView.ItemsSource = new List<Producto>(productosOriginales);


                // Refrescar datos relacionados con los productos
                await refrescarData.RefrescarProductosAsync(CategoriaId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en CargarProductosAsync - View_Categorias: {ex.Message}");
            }

           

        }


        #endregion

        private void FiltroTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = FiltroTextBox.Text.ToLower();

            // Si el cuadro de búsqueda está vacío, restauramos la lista original
            if (string.IsNullOrWhiteSpace(filtro))
            {
                ProductosListView.ItemsSource = new List<Producto>(productosOriginales); // Usamos una copia para evitar alteraciones
                return;
            }

            // Filtrar los productos cuyo Nombre o Descripcion contengan el texto ingresado
            var productosFiltrados = productosOriginales.Where(p =>
                p.Nombre.ToLower().Contains(filtro) ||
                p.Descripcion.ToLower().Contains(filtro)
            ).ToList();

            // Actualizar la fuente de datos con los resultados filtrados
            ProductosListView.ItemsSource = productosFiltrados;


        }

        #region filtro de mayor a menor 

        private Dictionary<string, int> estadoOrden = new Dictionary<string, int>
{
             { "Descripcion", 0 },
             { "Nombre", 0 },
    { "Precio", 0 },
    { "Ganancia", 0 },
    { "Cantidad", 0 },
    { "Total", 0 }
};



        private void OrdenarLista(object sender, RoutedEventArgs e)
        {
            if (ProductosListView.ItemsSource is List<Producto> productos && productosOriginales != null)
            {
                var textBlock = sender as TextBlock;
                if (textBlock == null) return;

                string propiedad = textBlock.Text.Replace(" ↓", "").Replace(" ↑", "").Trim(); // Limpiar texto dinámico

                // Resetear símbolos en todos los encabezados excepto el actual
                ResetearSimbolos(propiedad);

                // Definir la propiedad de orden según el TextBlock presionado
                Func<Producto, object>? selector = propiedad switch
                {
                    "Descripcion" => p => p.Descripcion,
                    "Nombre" => p => p.Nombre, // Agregamos la propiedad Nombre
                    "Precio" => p => p.PrecioCompra,
                    "Ganancia" => p => p.Ganancia,
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
                ProductosListView.ItemsSource = null;
                ProductosListView.ItemsSource = productos;
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
        #endregion












    }
}

