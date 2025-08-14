using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using AlmacenApp.Data;
using AlmacenApp.Models;
using Almacen;
using Microsoft.UI.Xaml.Input;
using System.Linq;
using Almacen.Models;
using Almacen.Views.View_Almacen;
using Microsoft.UI.Xaml.Hosting;
using Windows.UI.Composition;
using System.IO;
using Microsoft.UI.Xaml.Navigation;
using Almacen.Estilos_Configuracion.Estilos;
using static Almacen.Estilos_Configuracion.Estilos.ColorDeFondo;


namespace AlmacenApp.Views
{
    public sealed partial class AlmacenPage : Page
    {
        public ObservableCollection<Categoria> ListaCategorias { get; set; } = new ObservableCollection<Categoria>();

        private RefrescarData refrescarData;

        // Definir el evento ViewCategoriasCerrada
       // public event Action? ViewCategoriasCerrada;

        private ObservableCollection<Producto> ListaProductos = new ObservableCollection<Producto>();
        public ColorDeFondo Tema => ColorDeFondo.Instancia;
        public AlmacenPage()
        {
            this.InitializeComponent();          

            _ = ProcesarYCargarCategoriasAsync();

            // Inicializar con una instancia vacía
            refrescarData = new RefrescarData(new ObservableCollection<Categoria>(), new ObservableCollection<Producto>(), null!);

            this.Loaded += (s, e) =>
            {
                refrescarData = new RefrescarData(ListaCategorias, ListaProductos, CategoriasRepeater);
            };

            color();
        }


        private void color () { Almacen_Pagina.DataContext = ColorDeFondo.Instancia; }




        // =========================== CATEGORÍAS ===========================

        // ---------------- Métodos de botones y acciones ----------------


        private async void AgregarCategoria(object sender, RoutedEventArgs e)
        {
            // Ruta de la imagen predeterminada
            string rutaBase = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string rutaImagenPorDefecto = Path.Combine(rutaBase, "Almacen_Punto de venta", "Assets", "Imagenes", "Categoria", "Imagen_No_Cargada.png");


            Button seleccionarImagenButton = new Button
            {
                Content = "Seleccionar Imagen",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            Image imagenPreview = new Image
            {
                Width = 150,
                Height = 150,
                Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            TextBox nombreTextBox = new TextBox
            {
                PlaceholderText = "Nombre de la categoría",
                Margin = new Thickness(10)
            };

            byte[] imagenBytes = Array.Empty<byte>(); // Inicializa vacío

            seleccionarImagenButton.Click += async (s, args) =>
            {
                var picker = new FileOpenPicker();
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".png");

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow ?? throw new InvalidOperationException("La ventana principal no está inicializada"));

                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                StorageFile archivo = await picker.PickSingleFileAsync();
                if (archivo != null)
                {
                    // Leer los datos binarios de la imagen seleccionada
                    using (var stream = await archivo.OpenStreamForReadAsync())
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        imagenBytes = memoryStream.ToArray();
                    }

                    // Mostrar la imagen seleccionada en el preview
                    BitmapImage bitmapImage = new BitmapImage();
                    using (var stream = await archivo.OpenAsync(FileAccessMode.Read))
                    {
                        await bitmapImage.SetSourceAsync(stream);
                    }
                    imagenPreview.Source = bitmapImage;
                }
            };

            StackPanel panel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            panel.Children.Add(seleccionarImagenButton);
            panel.Children.Add(imagenPreview);
            panel.Children.Add(nombreTextBox);

            var dialog = new ContentDialog()
            {
                Title = "Agregar Categoría",
                Content = panel,
                CloseButtonText = "Cancelar",
                PrimaryButtonText = "Guardar",
                Background = Tema.FondoPrimario,
                Foreground = Tema.ColorDeLetra,
                BorderBrush = Tema.FondoTersario,
                BorderThickness = new Thickness(3)
            };

            if (this.XamlRoot != null)
            {
                dialog.XamlRoot = this.XamlRoot;
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string nombre = nombreTextBox.Text;

                // Si no se seleccionó una imagen, usar la imagen por defecto
                if (imagenBytes.Length == 0)
                {
                    using (var stream = File.OpenRead(rutaImagenPorDefecto))
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        imagenBytes = memoryStream.ToArray();
                    }

                    // Mostrar la imagen por defecto en el preview
                    BitmapImage bitmapImage = new BitmapImage();
                    using (var fileStream = File.OpenRead(rutaImagenPorDefecto))
                    {
                        await bitmapImage.SetSourceAsync(fileStream.AsRandomAccessStream());
                    }
                    imagenPreview.Source = bitmapImage;
                }

                // Guardar en la base de datos
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    await Bd_Categoria.AgregarCategoriaAsync(imagenBytes, nombre);
                    await Task.Delay(100); // Pequeña espera para asegurar que la BD procese la entrada

                    // REFRESCAR CATEGORÍAS INMEDIATAMENTE CON LA IMAGEN
                    await ProcesarYCargarCategoriasAsync();

                    // Actualizar el origen de datos de la UI para reflejar el cambio
                    if (CategoriasRepeater != null)
                    {
                        CategoriasRepeater.ItemsSource = ListaCategorias;
                    }
                }

            }
        }



        
        // ---------------- Funciones auxiliares ----------------
        private async void CargarCategorias()
        {
            if (CategoriasRepeater == null)
            {
                Debug.WriteLine("[ERROR] CategoriasRepeater no está inicializado." +
                    "Metodo CargarCategorias" +
                    "Almacenpage");
                return;
            }

            ListaCategorias.Clear();
            var categorias = await Bd_Categoria.ObtenerCategoriasAsync();          
            foreach (var categoria in categorias)
            {
                ListaCategorias.Add(categoria);
                
            }

            CategoriasRepeater.ItemsSource = ListaCategorias;
        }

        private async void AbrirViewCategorias(object sender, TappedRoutedEventArgs e)
        {
            MainContent.Visibility = Visibility.Collapsed;
            ViewCategoriasContainer.Visibility = Visibility.Visible;

            if (sender is StackPanel stackPanel && stackPanel.Tag is int categoriaId)
            {
                string nombreCategoria = "Nombre Desconocido"; // Valor por defecto
                BitmapImage imagenPreview = new BitmapImage(new Uri("ms-appx:///Assets/default.png")); // Imagen por defecto

                var categoria = await Bd_Categoria.ObtenerCategoriaPorIdAsync(categoriaId);

                if (categoria != null)
                {
                    nombreCategoria = categoria.Nombre;

                    if (categoria.Imagen != null)
                    {
                        using (var stream = new MemoryStream(categoria.Imagen))
                        {
                            imagenPreview = new BitmapImage();
                            await imagenPreview.SetSourceAsync(stream.AsRandomAccessStream());
                        }
                    }
                }

                // Pasar `CategoriasRepeater` al constructor de ViewCategorias
                var viewCategoriasPage = new ViewCategorias(categoriaId, nombreCategoria, imagenPreview, ListaCategorias, CategoriasRepeater);

                viewCategoriasPage.ViewCategoriasCerrada += async () =>
                {
                    MainContent.Visibility = Visibility.Visible;
                    ViewCategoriasContainer.Visibility = Visibility.Collapsed;

                    // Refrescar categorías al cerrar la vista
                    await refrescarData.RefrescarCategoriasAsync();
                    CategoriasRepeater.ItemsSource = ListaCategorias;
                };

                ViewCategoriasFrame.Content = viewCategoriasPage;
            }
        }


        private async Task<BitmapImage> ConvertirByteArrayAImagen(byte[] imagenBytes)
        {
            var bitmapImage = new BitmapImage();

            try
            {
                using (var stream = new MemoryStream(imagenBytes))
                {
                    await bitmapImage.SetSourceAsync(stream.AsRandomAccessStream());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error convirtiendo imagen:" +
                    $"ConvertirByteArrayAImagen" +
                    $"Almacenpage {ex.Message}");
            }

            return bitmapImage;
        }

        private async Task ProcesarYCargarCategoriasAsync()
        {
            try
            {
                var categorias = await Bd_Categoria.ObtenerCategoriasAsync();
                ListaCategorias.Clear();

                var categoriasUnicas = categorias
                    .GroupBy(c => c.Id) // Agrupar por ID único
                    .Select(g => g.First()) // Tomar solo uno por grupo
                    .ToList();

                foreach (var categoria in categoriasUnicas)
                {
                    if (categoria.Imagen != null && categoria.Imagen.Length > 0)
                    {
                        categoria.ImagenBitmap = await ConvertirByteArrayAImagen(categoria.Imagen);
                    }
                    else
                    {
                        categoria.ImagenBitmap = new BitmapImage(new Uri("ms-appx:///Assets/default.png"));
                    }

                    ListaCategorias.Add(categoria);
                }

                if (CategoriasRepeater != null)
                {
                    CategoriasRepeater.ItemsSource = ListaCategorias;
                  
                }

                if (CategoriasRepeater?.Layout == null)
                {
                    Debug.WriteLine("El Layout del CategoriasRepeater no está inicializado.");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error al procesar y cargar categorías:" +
                    $"Metodo ProcesarYCargarCategoriasAsync" +
                    $"Almacenpage {ex.Message}");
            }
        }

       


    }
}
