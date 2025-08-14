using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using AlmacenApp.Models;
using System.Threading;
using Almacen.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AlmacenApp.Data
{
    public static class Bd_Categoria
    {
        // -------------------------------------------------------------------------
        // Configuración de rutas y base de datos
        // -------------------------------------------------------------------------

        private static string dbPath = Path.Combine(A_Ruta_db.Ruta_BD, "BdAlmacen.db");




        // -------------------------------------------------------------------------
        // Constructor estático para inicializar directorios
        // -------------------------------------------------------------------------
        


        // -------------------------------------------------------------------------
        // Métodos de conexión e inicialización de la base de datos
        // -------------------------------------------------------------------------
        public static void Create_BD_Categoria()
        {
            try
            {
                // Usa la ruta personalizada definida en A_Ruta_db
                string dbPath = Path.Combine(A_Ruta_db.Ruta_BD, "BdAlmacen.db");

                // Asegúrate de que la carpeta donde se guardará la base de datos exista
                string? folderPath = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(folderPath) && !Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Conexión a la base de datos
                using (var connection = new SqliteConnection($"Data Source={dbPath};"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"CREATE TABLE IF NOT EXISTS Categorias (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                            Imagen BLOB, 
                            Nombre TEXT NOT NULL UNIQUE
                        );";

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] No se pudo inicializar la base de datos Bd_Categoria: {ex.Message}");
            }
        }


        // -------------------------------------------------------------------------
        // Métodos CRUD para Categorías
        // -------------------------------------------------------------------------

        /// <summary>
        /// Elimina una categoría de la base de datos junto con su imagen y productos asociados.
        /// </summary>
        /// <param name="id">ID de la categoría a eliminar</param>
        /// 
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        public static async Task EliminarCategoriaAsync(int id)
        {
            await semaphore.WaitAsync();
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        using (var command = connection.CreateCommand())
                        {
                            // Eliminar los productos relacionados con la categoría
                            command.CommandText = "DELETE FROM Productos WHERE CategoriaId = @id";
                            command.Parameters.AddWithValue("@id", id);
                            command.Transaction = (SqliteTransaction)transaction;
                            await command.ExecuteNonQueryAsync();
                        }

                        using (var command = connection.CreateCommand())
                        {
                            // Eliminar la categoría
                            command.CommandText = "DELETE FROM Categorias WHERE Id = @id";
                            command.Parameters.AddWithValue("@id", id);
                            command.Transaction = (SqliteTransaction)transaction;
                            await command.ExecuteNonQueryAsync();
                        }

                        // Confirmar los cambios en la transacción
                        await transaction.CommitAsync();
                    }
                }
                Debug.WriteLine($"[INFO Bd_Categoria] Categoría {id} eliminada correctamente. Bd_Categoria");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR Bd_Categoria] No se pudo eliminar la categoría Bd_Categoria: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        }


        /// <summary>
        /// Modifica una categoría existente en la base de datos.
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <param name="nuevaImagen">Ruta de la nueva imagen</param>
        /// <param name="nuevoNombre">Nuevo nombre de la categoría</param>
        public static async Task Modificar_Categoria(int id, byte[]? nuevaImagen, string nuevoNombre)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE Categorias SET Nombre = @nombre, Imagen = @imagen WHERE Id = @id";
                            command.Parameters.AddWithValue("@nombre", nuevoNombre);
                            command.Parameters.AddWithValue("@imagen", nuevaImagen != null ? (object)nuevaImagen : DBNull.Value); // Solución aquí
                            command.Parameters.AddWithValue("@id", id);
                            command.Transaction = (SqliteTransaction)transaction;

                            await command.ExecuteNonQueryAsync();
                        }
                        await transaction.CommitAsync();
                    }
                }
                Debug.WriteLine($"[INFO] Categoría {id} actualizada correctamente. Bd_Categoria");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] No se pudo actualizar la categoría Bd_Categoria: {ex.Message}");
            }
        }

        /// <summary>
        /// Agrega una nueva categoría a la base de datos.
        /// </summary>
        /// <param name="imagenRuta">Ruta de la imagen de la categoría.</param>
        /// <param name="nombre">Nombre de la categoría.</param>
        /// <remarks>
        /// Si la imagen ya existe, se renombra para evitar conflictos.
        /// </remarks>
        public static async Task AgregarCategoriaAsync(byte[] imagen, string nombre)
        {
            try
            {
                A_Carga_De_Tablas.Creacion_Tablas_BD();
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    string insertCommand = "INSERT INTO Categorias (Imagen, Nombre) VALUES (@Imagen, @Nombre);";
                    using (var command = new SqliteCommand(insertCommand, connection))
                    {
                        // Usar parámetros para la imagen y el nombre
                        command.Parameters.AddWithValue("@Imagen", imagen);
                        command.Parameters.AddWithValue("@Nombre", nombre);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] No se pudo agregar la categoría Bd_Categoria: {ex.Message}");
            }
        }



        /// <summary>
        /// Obtiene una categoría por su ID.
        /// </summary>
        /// <param name="id">Identificador único de la categoría.</param>
        /// <returns>Un objeto <see cref="Categoria"/> si se encuentra, de lo contrario, null.</returns>
        public static async Task<Categoria?> ObtenerCategoriaPorIdAsync(int id)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Id, Nombre, Imagen FROM Categorias WHERE Id = @id";
                        command.Parameters.AddWithValue("@id", id);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Categoria
                                {
                                    Id = reader.GetInt32(0),
                                    Nombre = reader.GetString(1),
                                    Imagen = reader.IsDBNull(2) ? null : (byte[])reader[2] // Recupera la imagen como byte[].
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] No se pudo obtener la categoría Bd_Categoria: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Obtiene la lista de todas las categorías almacenadas en la base de datos.
        /// </summary>
        /// <returns>Lista de objetos de tipo Categoria</returns>
        public static async Task<List<Categoria>> ObtenerCategoriasAsync()
        {
            List<Categoria> categorias = new List<Categoria>();

            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    using (var command = new SqliteCommand("SELECT Id, Imagen, Nombre FROM Categorias;", connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categorias.Add(new Categoria
                            {
                                Id = reader.GetInt32(0),
                                Imagen = reader.IsDBNull(1) ? null : (byte[])reader[1],
                                Nombre = reader.GetString(2)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] No se pudo obtener las categorías Bd_Categoria: {ex.Message}");
            }

            return categorias;
        }

    }
}
