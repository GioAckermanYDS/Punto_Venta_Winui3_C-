using Microsoft.Data.Sqlite;
using System;
using System.Diagnostics;
using System.IO;

namespace Almacen.Data
{
    public class Perfil_User
    {

        private static string dbPath = Path.Combine(A_Ruta_db.Ruta_BD, "BdAlmacen.db");

        #region BD logo imagen


        // Método para inicializar la tabla
        public static void Crear_BD_TablaImg()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    // Crear la tabla si no existe
                    command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Perfil_User (
                    Id INTEGER PRIMARY KEY CHECK (Id = 1), -- Solo permite un único registro
                    Imagen BLOB NOT NULL
                );
            ";
                    command.ExecuteNonQuery();

                    // Verificar si la tabla tiene datos
                    command.CommandText = "SELECT COUNT(*) FROM Perfil_User;";
                    var count = Convert.ToInt32(command.ExecuteScalar());

                    if (count == 0) // Si no hay datos, insertar la imagen predeterminada
                    {
                        string rutaBase = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string imagePath = Path.Combine(rutaBase, "Almacen_Punto de venta", "Assets", "Imagenes", "Logo", "logo.jpg");


                        if (File.Exists(imagePath)) // Verificar que la imagen existe
                        {
                            byte[] imageBytes = File.ReadAllBytes(imagePath);
                            command.CommandText = "INSERT INTO Perfil_User (Id, Imagen) VALUES (1, @imagen);";
                            command.Parameters.AddWithValue("@imagen", imageBytes);
                            command.ExecuteNonQuery();
                            Debug.WriteLine("Imagen predeterminada agregada a la tabla.");
                        }
                        else
                        {
                            Debug.WriteLine($"Error: La imagen en la ruta '{imagePath}' no existe.");
                        }
                    }
                   
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN CrearTabla]");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }



        public static void GuardarOActualizarLogo(byte[] nuevaImagen, MainWindow mainWindowInstance)
        {
            if (nuevaImagen == null || nuevaImagen.Length == 0)
            {
                Debug.WriteLine("[ERROR] La imagen proporcionada está vacía o es nula.");
                return;
            }

            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // Usar una sola consulta para determinar si existe el registro
                    using (var checkCommand = connection.CreateCommand())
                    {
                        checkCommand.CommandText = "SELECT EXISTS(SELECT 1 FROM Perfil_User WHERE Id = 1)";
                        bool existeRegistro = Convert.ToBoolean(checkCommand.ExecuteScalar());

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = existeRegistro
                                ? "UPDATE Perfil_User SET Imagen = @img WHERE Id = 1"
                                : "INSERT INTO Perfil_User (Id, Imagen) VALUES (1, @img)";

                            command.Parameters.AddWithValue("@img", nuevaImagen);
                            command.ExecuteNonQuery();
                        }
                    }

                    Debug.WriteLine("🚀 Imagen del logo guardada o actualizada correctamente.");

                    // 🔄 Llamar a `CargarLogo()` en `MainWindow` para actualizar la UI
                    mainWindowInstance.CargarLogo();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN GuardarOActualizarLogo]");
                Debug.WriteLine("No se pudo guardar o actualizar la imagen del logo.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }
        #endregion


        #region Configuracio de tema fondo

        public static void Crear_BD_TablaTemasFondo()
        {
            try
            {
               

                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    // Verificar si la tabla existe
                    command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Temas_Fondo';";
                    var count = Convert.ToInt32(command.ExecuteScalar());

                    if (count == 0) // Si no existe, crear la tabla y agregar datos iniciales
                    {
                        command.CommandText = @"
                CREATE TABLE Temas_Fondo (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                    Oscuro INTEGER NOT NULL, 
                    Claro INTEGER NOT NULL, 
                    Sepia INTEGER NOT NULL
                );";
                        command.ExecuteNonQuery();

                        Debug.WriteLine("✅ Tabla 'Temas_Fondo' creada correctamente.");

                        // Insertar valores predeterminados
                        command.CommandText = "INSERT INTO Temas_Fondo (Oscuro, Claro, Sepia) VALUES (1, 0, 0);";
                        command.ExecuteNonQuery();

                        Debug.WriteLine("✅ Valores predeterminados insertados en la tabla 'Temas_Fondo'.");
                    }
                    
                } // La conexión se cierra automáticamente aquí
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR EN CrearTablaTemasFondo]");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
            }
        }

        public static void ActualizarTemaSeleccionado(string nombreTema)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    // Establecer todos los temas en "0" (falso)
                    command.CommandText = "UPDATE Temas_Fondo SET Oscuro = 0, Claro = 0, Sepia = 0;";
                    command.ExecuteNonQuery();

                    // Activar solo el tema seleccionado
                    switch (nombreTema)
                    {
                        case "Oscuro":
                            command.CommandText = "UPDATE Temas_Fondo SET Oscuro = 1;";
                            break;
                        case "Claro":
                            command.CommandText = "UPDATE Temas_Fondo SET Claro = 1;";
                            break;
                        case "Sepia":
                            command.CommandText = "UPDATE Temas_Fondo SET Sepia = 1;";
                            break;
                    }

                    command.ExecuteNonQuery();
                    Debug.WriteLine($"✅ Tema '{nombreTema}' activado en la base de datos.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR EN ActualizarTemaSeleccionado]");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
            }
        }

        public static string ObtenerTemaGuardado()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    // Consultar cuál tema tiene el valor "1"
                    command.CommandText = @"
                SELECT 
                    CASE 
                        WHEN Oscuro = 1 THEN 'Oscuro'
                        WHEN Claro = 1 THEN 'Claro'
                        WHEN Sepia = 1 THEN 'Sepia'
                        ELSE 'Claro' -- Tema por defecto si no hay selección
                    END
                FROM Temas_Fondo;";

                    var temaGuardado = command.ExecuteScalar()?.ToString();

                    if (!string.IsNullOrEmpty(temaGuardado))
                    {
                        
                        return temaGuardado;
                    }
                    else
                    {
                        Debug.WriteLine("⚠️ No se encontró ningún tema guardado. Se usará el tema predeterminado.");
                        return "Claro"; // Tema por defecto
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR EN ObtenerTemaGuardado]");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                return "Claro"; // En caso de error, usar tema claro por defecto
            }
        }






        #endregion

    }

}
