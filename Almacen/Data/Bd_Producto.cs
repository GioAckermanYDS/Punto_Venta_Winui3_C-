using Almacen.Models;
using AlmacenApp.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlmacenApp.Data;
using Almacen.Data;

namespace AlmacenApp.Data
{
    public static class Bd_Producto
    {

        private static string dbPath = Path.Combine(A_Ruta_db.Ruta_BD, "BdAlmacen.db");


        public static void Create_BD_Producto()
        {
            try
            {
                
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Productos (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            CategoriaId INTEGER NOT NULL,
                            Nombre TEXT NOT NULL,
                            Descripcion TEXT,
                            PrecioCompra REAL NOT NULL,
                            Total REAL NOT NULL,
                            Cantidad INTEGER NOT NULL,
                            StockMinimo INTEGER NOT NULL,
                            Eleccion_Calculo BOOLEAN NOT NULL DEFAULT 0,
                            Precio_Fijo REAL NOT NULL DEFAULT 0,
                            Precio_Porcentaje REAL NOT NULL DEFAULT 0,
                            FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id) ON DELETE CASCADE
                        );

                        CREATE TABLE IF NOT EXISTS Referencias (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Codigo TEXT NOT NULL UNIQUE
                        );

                        CREATE TABLE IF NOT EXISTS ProductoReferencias (
                            ProductoId INTEGER NOT NULL,
                            ReferenciaId INTEGER NOT NULL,
                            FOREIGN KEY (ProductoId) REFERENCES Productos(ID),
                            FOREIGN KEY (ReferenciaId) REFERENCES Referencias(Id)
                        );
                        ";
                    command.ExecuteNonQuery();
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Producto]");
                Debug.WriteLine("No se pudo inicializar la base de datos.");
                Debug.WriteLine("Método: InitializeDatabase");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }




        #region Funcion de crud
        public static int CreateProducto(Producto producto)
        {
           
            int productoId = 0; // Inicializar con valor predeterminado

            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                    INSERT INTO Productos (
                        CategoriaId, Nombre, Descripcion, PrecioCompra, Total, Cantidad, StockMinimo, 
                        Eleccion_Calculo, Precio_Fijo, Precio_Porcentaje
                    ) 
                    VALUES (
                        @CategoriaId, @Nombre, @Descripcion, @PrecioCompra, @Total, @Cantidad, @StockMinimo, 
                        @Eleccion_Calculo, @PrecioFijo, @PrecioPorcentaje
                    );
                    SELECT last_insert_rowid();";

                    // Agregar los parámetros al comando
                    command.Parameters.AddWithValue("@CategoriaId", producto.CategoriaId);
                    command.Parameters.AddWithValue("@Nombre", producto.Nombre);
                    command.Parameters.AddWithValue("@Descripcion", producto.Descripcion);
                    command.Parameters.AddWithValue("@PrecioCompra", producto.PrecioCompra);
                    command.Parameters.AddWithValue("@Total", producto.Total);
                    command.Parameters.AddWithValue("@Cantidad", producto.Cantidad);
                    command.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
                    command.Parameters.AddWithValue("@Eleccion_Calculo", producto.Eleccion_Calculo);
                    command.Parameters.AddWithValue("@PrecioFijo", producto.Precio_Fijo);
                    command.Parameters.AddWithValue("@PrecioPorcentaje", producto.Precio_Porcentaje);

                    // Ejecutar el comando y recuperar el ID generado
                    productoId = Convert.ToInt32(command.ExecuteScalar() ?? 0);

                    Debug.WriteLine($"Producto creado exitosamente con ID {productoId}.");
                }
            }
            catch (SqliteException ex) // Captura errores específicos de SQLite
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Producto]");
                Debug.WriteLine("No se pudo crear el producto.");
                Debug.WriteLine("Método: CreateProducto");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"Código de error: {ex.ErrorCode}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (InvalidOperationException ex) // Captura errores de operación inválida
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Producto]");
                Debug.WriteLine("Operación inválida al crear el producto.");
                Debug.WriteLine("Método: CreateProducto");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (Exception ex) // Captura errores generales
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Producto]");
                Debug.WriteLine("Error inesperado al crear el producto.");
                Debug.WriteLine("Método: CreateProducto");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }

            return productoId; // Retornar el ID
        }

        public static async Task<int> EliminarProducto(int productoId)
        {
            if (productoId <= 0)
            {
                throw new ArgumentException("El ID del producto no es válido.", nameof(productoId));
            }

            int filasAfectadas = 0;
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "DELETE FROM Productos WHERE Id = @ProductoId;";
                        command.Parameters.AddWithValue("@ProductoId", productoId);

                        filasAfectadas = await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar el producto: {ex.Message}");
            }

            return filasAfectadas;
        }


        public static async Task<int> Modificar_Producto(int productoId, string nuevoNombre, string nuevaDescripcion, decimal precioCompra, decimal precioFijo, string eleccionCalculo, decimal precioPorcentaje, int cantidad)
        {
            int filasAfectadas = 0;

            try
            {
                // Verificar que la base de datos exista
                if (string.IsNullOrEmpty(dbPath) || !File.Exists(dbPath))
                {
                    throw new InvalidOperationException($"La base de datos no se encuentra en la ruta especificada: {dbPath}");
                }

                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        // Comando SQL para modificar los campos
                        command.CommandText = @"
                        UPDATE Productos
                        SET Nombre = @Nombre, 
                            Descripcion = @Descripcion, 
                            PrecioCompra = @PrecioCompra, 
                            Precio_Fijo = @PrecioFijo, 
                            Eleccion_Calculo = @EleccionCalculo, 
                            Precio_Porcentaje = @PrecioPorcentaje,
                            Cantidad = @Cantidad
                        WHERE ID = @ProductoId;";

                        // Agregar los parámetros
                        command.Parameters.AddWithValue("@Nombre", nuevoNombre);
                        command.Parameters.AddWithValue("@Descripcion", nuevaDescripcion);
                        command.Parameters.AddWithValue("@PrecioCompra", precioCompra);
                        command.Parameters.AddWithValue("@PrecioFijo", precioFijo);
                        command.Parameters.AddWithValue("@EleccionCalculo", eleccionCalculo);
                        command.Parameters.AddWithValue("@PrecioPorcentaje", precioPorcentaje);
                        command.Parameters.AddWithValue("@Cantidad", cantidad);
                        command.Parameters.AddWithValue("@ProductoId", productoId);

                        // Ejecutar el comando y obtener el número de filas afectadas
                        filasAfectadas = await command.ExecuteNonQueryAsync();
                    }
                }
               
            }
            catch (SqliteException ex) // Captura errores de base de datos
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Producto]");
                Debug.WriteLine("No se pudo modificar el producto.");
                Debug.WriteLine("Método: Modificar_Producto");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"Código de error: {ex.ErrorCode}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (InvalidOperationException ex) // Captura errores de operaciones inválidas
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Producto]");
                Debug.WriteLine("Error de operación al modificar el producto.");
                Debug.WriteLine("Método: Modificar_Producto");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (Exception ex) // Captura errores generales
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Producto]");
                Debug.WriteLine("Error inesperado al modificar el producto.");
                Debug.WriteLine("Método: Modificar_Producto");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }

            return filasAfectadas;
        }

        public static async Task<int> RealizarVenta(int productoId, int nuevaCantidad)
        {
            int filasAfectadas = 0;

            try
            {
                // Verificar que la base de datos exista
                if (string.IsNullOrEmpty(dbPath) || !File.Exists(dbPath))
                {
                    throw new InvalidOperationException($"La base de datos no se encuentra en la ruta especificada: {dbPath}");
                }

                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        // 🔥 SOLO ACTUALIZA CANTIDAD, sin modificar otros datos
                        command.CommandText = @"
                UPDATE Productos
                SET Cantidad = @Cantidad
                WHERE ID = @ProductoId;";

                        // Agregar los parámetros
                        command.Parameters.AddWithValue("@Cantidad", nuevaCantidad);
                        command.Parameters.AddWithValue("@ProductoId", productoId);

                        // Ejecutar el comando y obtener el número de filas afectadas
                        filasAfectadas = await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (SqliteException ex) // Captura errores de base de datos
            {
                Debug.WriteLine($"❌ Error en RealizarVenta: {ex.Message}");
            }
            catch (Exception ex) // Captura errores generales
            {
                Debug.WriteLine($"❌ Error inesperado en RealizarVenta: {ex.Message}");
            }

            return filasAfectadas;
        }


        #endregion

        #region Funciones de recoleccion de informacion de la db_productos 
        public static async Task<List<Producto>> GetProductosPorCategoriaAsync(int categoriaId, int productoId = 0)
        {
            var productos = new List<Producto>();

            try
            {
                // Verificar que la base de datos exista
                if (string.IsNullOrEmpty(dbPath) || !File.Exists(dbPath))
                {
                    throw new InvalidOperationException($"La base de datos no se encuentra en la ruta especificada: {dbPath}");
                }

                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        // Construir el comando SQL dinámicamente según los parámetros
                        if (productoId > 0)
                        {
                            command.CommandText = @"
                    SELECT 
                        ID, CategoriaId, Nombre, Descripcion, PrecioCompra, Cantidad, StockMinimo, 
                        Eleccion_Calculo, Precio_Fijo, Precio_Porcentaje
                    FROM 
                        Productos 
                    WHERE 
                        ID = @ProductoId;";
                            command.Parameters.AddWithValue("@ProductoId", productoId);
                        }
                        else
                        {
                            command.CommandText = @"
                    SELECT 
                        ID, CategoriaId, Nombre, Descripcion, PrecioCompra, Cantidad, StockMinimo, 
                        Eleccion_Calculo, Precio_Fijo, Precio_Porcentaje
                    FROM 
                        Productos 
                    WHERE 
                        CategoriaId = @CategoriaId;";
                            command.Parameters.AddWithValue("@CategoriaId", categoriaId);
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                productos.Add(new Producto
                                {
                                    ID = reader.GetInt32(0),
                                    CategoriaId = reader.GetInt32(1),
                                    Nombre = reader.GetString(2),
                                    Descripcion = reader.GetString(3),
                                    PrecioCompra = reader.GetInt32(4),
                                    Cantidad = reader.GetInt32(5),
                                    StockMinimo = reader.GetInt32(6),
                                    Eleccion_Calculo = reader.GetBoolean(7), // Campo ajustado
                                    Precio_Fijo = reader.GetDouble(8),        // Nuevo campo
                                    Precio_Porcentaje = reader.GetDouble(9)  // Nuevo campo
                                });
                            }
                        }
                    }
                }
            }
            catch (SqliteException ex) // Captura errores específicos de SQLite
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Producto]");
                Debug.WriteLine("Error de base de datos en GetProductosPorCategoriaAsync.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"Código de error: {ex.ErrorCode}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (InvalidOperationException ex) // Captura errores de operación inválida
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Producto]");
                Debug.WriteLine("Error de operación en GetProductosPorCategoriaAsync.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (Exception ex) // Captura errores generales
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Producto]");
                Debug.WriteLine("Error inesperado en GetProductosPorCategoriaAsync.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }

            return productos;
        }

        public static async Task<Dictionary<int, List<Producto>>> GetTodosLosProductosPorCategoriaAsync()
        {
            var productosPorCategoria = new Dictionary<int, List<Producto>>();

            try
            {
                // Verificar que la base de datos exista
                if (string.IsNullOrEmpty(dbPath) || !File.Exists(dbPath))
                {
                    throw new InvalidOperationException($"La base de datos no se encuentra en la ruta especificada: {dbPath}");
                }

                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                SELECT 
                    ID, CategoriaId, Nombre, Descripcion, PrecioCompra, Cantidad, StockMinimo, 
                    Eleccion_Calculo, Precio_Fijo, Precio_Porcentaje
                FROM Productos;"; // Obtiene todos los productos

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var producto = new Producto
                                {
                                    ID = reader.GetInt32(0),
                                    CategoriaId = reader.GetInt32(1),
                                    Nombre = reader.GetString(2),
                                    Descripcion = reader.GetString(3),
                                    PrecioCompra = reader.GetInt32(4),
                                    Cantidad = reader.GetInt32(5),
                                    StockMinimo = reader.GetInt32(6),
                                    Eleccion_Calculo = reader.GetBoolean(7),
                                    Precio_Fijo = reader.GetDouble(8),
                                    Precio_Porcentaje = reader.GetDouble(9)
                                };

                                if (!productosPorCategoria.ContainsKey(producto.CategoriaId))
                                {
                                    productosPorCategoria[producto.CategoriaId] = new List<Producto>();
                                }

                                productosPorCategoria[producto.CategoriaId].Add(producto);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en GetTodosLosProductosPorCategoriaAsync: {ex.Message}");
            }

            return productosPorCategoria;
        }

        #endregion

        #region Funciones asociadadas con productos 

        public static void ReducirStock(int productoId, int cantidadReducida)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // Obtener cantidad actual y umbral
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Cantidad, StockMinimo FROM Productos WHERE ID = @ProductoId;";
                    command.Parameters.AddWithValue("@ProductoId", productoId);

                    int cantidadActual = 0;
                    int umbral = 0;

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cantidadActual = reader.GetInt32(0);
                            umbral = reader.GetInt32(1);
                        }
                        else
                        {
                            throw new KeyNotFoundException($"El producto con ID {productoId} no fue encontrado.");
                        }
                    }

                    // Reducir cantidad
                    int nuevaCantidad = Math.Max(cantidadActual - cantidadReducida, 0);

                    var updateCommand = connection.CreateCommand();
                    updateCommand.CommandText = "UPDATE Productos SET Cantidad = @NuevaCantidad WHERE ID = @ProductoId;";
                    updateCommand.Parameters.AddWithValue("@NuevaCantidad", nuevaCantidad);
                    updateCommand.Parameters.AddWithValue("@ProductoId", productoId);
                    updateCommand.ExecuteNonQuery();

                    Debug.WriteLine("-----------------------------------------------------------------");
                    Debug.WriteLine($"Stock actualizado para el producto ID {productoId}. Nueva cantidad: {nuevaCantidad}");
                    Debug.WriteLine("-----------------------------------------------------------------");

                    // Crear alerta si es necesario
                    if (nuevaCantidad <= umbral)
                    {
                        Bd_AlertaStock.CrearAlerta(new AlertaStock
                        {
                            ProductoId = productoId,
                            CantidadActual = nuevaCantidad,
                            Umbral = umbral,
                            FechaAlerta = DateTime.Now
                        });

                        Debug.WriteLine("-----------------------------------------------------------------");
                        Debug.WriteLine($"⚠️ Se ha generado una alerta de stock bajo para el producto ID {productoId}.");
                        Debug.WriteLine("-----------------------------------------------------------------");
                    }
                }
            }
            catch (SqliteException ex) // Captura errores de base de datos
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Producto]");
                Debug.WriteLine("Error de base de datos en ReducirStock.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"Código de error: {ex.ErrorCode}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (KeyNotFoundException ex) // Captura si el producto no existe
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Producto]");
                Debug.WriteLine("Intento de reducir stock de un producto inexistente.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
            catch (Exception ex) // Captura errores generales
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Producto]");
                Debug.WriteLine("Error inesperado en ReducirStock.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }




        #endregion
    }
}
