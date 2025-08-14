using AlmacenApp.Data;
using AlmacenApp.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Almacen.Models
{
    public class RefrescarData
    {
        #region Variables privadas
        private readonly object lockCategorias = new object();
        private readonly ObservableCollection<Categoria> ListaCategorias;
        private readonly ObservableCollection<Producto> ListaProductos;
        private readonly ItemsRepeater categoriasRepeater;
        private bool isRefreshingCategorias = false; // Bandera para evitar ejecución repetida

        #endregion

        #region Constructor
        public RefrescarData(ObservableCollection<Categoria> listaCategorias, ObservableCollection<Producto> listaProductos, ItemsRepeater repeater)
        {
            ListaCategorias = listaCategorias ?? throw new ArgumentNullException(nameof(listaCategorias));
            ListaProductos = listaProductos ?? throw new ArgumentNullException(nameof(listaProductos));
            categoriasRepeater = repeater;
        }
        #endregion

        #region Método para obtener categorías desde la base de datos
        private readonly SemaphoreSlim semaphoreCategorias = new SemaphoreSlim(1, 1);

        public async Task ObtenerCategoriasAsync()
        {
            if (isRefreshingCategorias) return; // Si ya está refrescando, evita otra ejecución

            try
            {
                isRefreshingCategorias = true; // Activamos bandera

                List<Categoria> categorias = await Bd_Categoria.ObtenerCategoriasAsync();
              

                await semaphoreCategorias.WaitAsync();
                try
                {
                    ListaCategorias.Clear();

                    foreach (var categoria in categorias)
                    {
                        if (categoria.Imagen != null && categoria.Imagen.Length > 0)
                        {
                            using (var stream = new MemoryStream(categoria.Imagen))
                            {
                                var bitmapImage = new BitmapImage();
                                await bitmapImage.SetSourceAsync(stream.AsRandomAccessStream());
                                categoria.ImagenBitmap = bitmapImage;
                            }
                        }
                        else
                        {
                            categoria.ImagenBitmap = new BitmapImage(new Uri("ms-appx:///Assets/default.png"));
                        }

                        ListaCategorias.Add(categoria);
                    }
                }
                finally
                {
                    semaphoreCategorias.Release();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error al obtener categorías: {ex.Message}");
            }
            finally
            {
                isRefreshingCategorias = false; // Bandera vuelve a estar disponible
            }
        }
        #endregion

        #region Método para refrescar solo las categorías y actualizar UI
        public async Task RefrescarCategoriasAsync()
        {
            if (isRefreshingCategorias) return; // Evitar ejecución duplicada

            await ObtenerCategoriasAsync();

            if (categoriasRepeater != null)
            {
                categoriasRepeater.ItemsSource = null; // Rompe referencia anterior
                categoriasRepeater.ItemsSource = ListaCategorias; // Asigna lista actualizada
              
            }
        }
        #endregion

        #region Método para obtener productos por categoría desde la base de datos
        private bool isRefreshingProductos = false;

        public async Task ObtenerProductosPorCategoriaAsync(int categoriaId)
        {
            if (isRefreshingProductos) return; // Evita ejecución duplicada

            try
            {
                isRefreshingProductos = true; // Activamos bandera

                List<Producto> productos = await Bd_Producto.GetProductosPorCategoriaAsync(categoriaId);
                ListaProductos.Clear();

                foreach (var producto in productos)
                {
                    ListaProductos.Add(producto);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error al obtener productos: {ex.Message}");
            }
            finally
            {
                isRefreshingProductos = false; // Bandera vuelve a estar disponible
            }
        }
        #endregion

        #region Método para refrescar productos
        public async Task RefrescarProductosAsync(int categoriaId)
        {
            if (isRefreshingProductos) return; // Evita doble ejecución

            await ObtenerProductosPorCategoriaAsync(categoriaId);
        }
        #endregion

        #region Método combinado para refrescar categorías y productos
        public async Task RefrescarTodoAsync(int categoriaId)
        {
            try
            {
                await RefrescarCategoriasAsync();
                await RefrescarProductosAsync(categoriaId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Error al refrescar todo: {ex.Message}");
            }
        }
        #endregion
    }
}
