using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using Almacen.Models.Modelo_Validar_Usuario;
using Almacen.Models;
using Almacen.Estilos_Configuracion.Estilos;
using Almacen.Models.Validar_Usuario;




namespace Almacen.Data.Validar_Ingreso
{
    class Control_Usuarios
    {
        private static string dbPath = Path.Combine(A_Ruta_db.Ruta_BD, "BdAlmacen.db");

        #region creacion del perfil     
        public static void Create_BD_Usuarios()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Usuarios (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre TEXT NOT NULL,
                    Cedula TEXT NOT NULL UNIQUE,
                    Usuario TEXT NOT NULL UNIQUE,
                    Contraseña TEXT NOT NULL,
                    Perfil INTEGER NOT NULL CHECK (Perfil IN (0,1)) -- 0 = Admin, 1 = Vendedor
                );
            ";
                    command.ExecuteNonQuery();

                    // ✅ Verificar si ya existen usuarios
                    var checkCommand = connection.CreateCommand();
                    checkCommand.CommandText = "SELECT COUNT(*) FROM Usuarios;";
                    long cantidadUsuarios = (long)checkCommand.ExecuteScalar();

                    // ✅ Si no hay usuarios, insertar uno por defecto
                    if (cantidadUsuarios == 0)
                    {
                        var insertCommand = connection.CreateCommand();
                        insertCommand.CommandText = @"
                    INSERT INTO Usuarios (Nombre, Cedula, Usuario, Contraseña, Perfil)
                    VALUES ('Administrador', '0000000000', 'admin', '0', 0);
                ";
                        insertCommand.ExecuteNonQuery();
                        Debug.WriteLine("✅ Usuario administrador por defecto insertado.");
                    }
                    else
                    {
                        Debug.WriteLine("ℹ️ Ya existen usuarios en la tabla. No se insertó usuario por defecto.");
                    }
                }

                // ✅ Actualizar la lista de usuarios en memoria o UI si aplica
                Extracion_Usuario();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Usuarios]");
                Debug.WriteLine("No se pudo crear la tabla de usuarios o insertar datos.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }

        public static List<(string Usuario, string Contraseña)> Extracion_Usuario()
        {
            var listaUsuarios = new List<(string Usuario, string Contraseña)>();

            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Usuario, Contraseña FROM Usuarios";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var usuario = reader["Usuario"]?.ToString() ?? string.Empty;
                            var contraseña = reader["Contraseña"]?.ToString() ?? string.Empty;

                            listaUsuarios.Add((usuario, contraseña));

                            // Imprimir en la consola de depuración
                            //Debug.WriteLine($"Usuario: {usuario}, Contraseña: {contraseña}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN LA CLASE Bd_Usuarios]");
                Debug.WriteLine("No se pudo extraer los usuarios.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }

            return listaUsuarios;
        }

        public static List<Model_Validar_Usuario> ObtenerPerfilesUsuarios()
        {
            var listaPerfiles = new List<Model_Validar_Usuario>();

            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT ID, Nombre, Cedula, Usuario, Contraseña, Perfil FROM Usuarios";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listaPerfiles.Add(new Model_Validar_Usuario
                            {
                                ID = Convert.ToInt32(reader["ID"]), // ✅ Extraer ID correctamente
                                Nombre = reader["Nombre"]?.ToString() ?? string.Empty,
                                Cedula = reader["Cedula"]?.ToString() ?? string.Empty,
                                Usuario = reader["Usuario"]?.ToString() ?? string.Empty,
                                Contraseña = reader["Contraseña"]?.ToString() ?? string.Empty,
                                Perfil = Convert.ToInt32(reader["Perfil"] ?? 0)
                            });
                        }
                    }
                }

                // 🔄 Notificar que la lista se ha actualizado

                RefrescarData.Actualizar_Nit_Empresa(); // 🔥 Aquí refrescamos la UI automáticamente

            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ [ERROR AL OBTENER PERFILES]");
                Debug.WriteLine($"Mensaje de error Model_Validar_Usuario: {ex.Message}");
            }

            return listaPerfiles;
        }

        public static void AgregarNuevoUsuario(string nombre, string cedula, string usuario, string contraseña, int perfil)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                INSERT INTO Usuarios (Nombre, Cedula, Usuario, Contraseña, Perfil) 
                VALUES (@nombre, @cedula, @usuario, @contraseña, @perfil)"; // Perfil dinámico

                        command.Parameters.AddWithValue("@nombre", nombre);
                        command.Parameters.AddWithValue("@cedula", cedula);
                        command.Parameters.AddWithValue("@usuario", usuario);
                        command.Parameters.AddWithValue("@contraseña", contraseña);
                        command.Parameters.AddWithValue("@perfil", perfil); // Nuevo parámetro agregado

                        command.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ [ERROR AL INSERTAR USUARIO]");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        public static void ActualizarUsuario(int id, string nombre, string cedula, string usuario, string contrasena, int perfil)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    // Validar si el usuario existe antes de actualizar
                    bool usuarioExiste = false;
                    using (var checkCommand = connection.CreateCommand())
                    {
                        checkCommand.CommandText = "SELECT COUNT(*) FROM Usuarios WHERE ID = @id";
                        checkCommand.Parameters.AddWithValue("@id", id);
                        usuarioExiste = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
                    }

                    if (!usuarioExiste)
                    {
                        Debug.WriteLine($"⚠️ Usuario con ID {id} no encontrado en la base de datos.");
                        return;
                    }

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                UPDATE Usuarios 
                SET Nombre = @nombre, Cedula = @cedula, Usuario = @usuario, Contraseña = @contraseña, Perfil = @perfil
                WHERE ID = @id";

                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@nombre", nombre);
                        command.Parameters.AddWithValue("@cedula", cedula);
                        command.Parameters.AddWithValue("@usuario", usuario);
                        command.Parameters.AddWithValue("@contraseña", contrasena);
                        command.Parameters.AddWithValue("@perfil", perfil);

                        int filasAfectadas = command.ExecuteNonQuery();
                        if (filasAfectadas > 0)
                        {
                            Debug.WriteLine($"✅ Usuario {usuario} actualizado correctamente en la base de datos con perfil {perfil}.");
                        }
                        else
                        {
                            Debug.WriteLine($"⚠️ No se actualizó ningún usuario. Verifica que el ID {id} existe.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ [ERROR AL ACTUALIZAR USUARIO]");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine($"Datos enviados: ID={id}, Nombre={nombre}, Cedula={cedula}, Usuario={usuario}, Contraseña={contrasena}, Perfil={perfil}");
            }
        }
        #endregion


        #region Perfil Ultima Decicion   

        public static void Create_BD_Ultimo_Inicio_Usuarios()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                CREATE TABLE IF NOT EXISTS UltimoInicio (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Tipo_Perfil_Usuario_Ultimo_Inicio TEXT NOT NULL,
                    Nombre_Usuario_Ultimo_Inicio TEXT NOT NULL,
                    Usuario_Ultimo_Inicio TEXT NOT NULL,
                    Cedula_Usuario_Ultimo_Inicio TEXT NOT NULL
                );
            ";
                    command.ExecuteNonQuery();

                    // ✅ Verificar si ya hay datos en la tabla
                    var checkCommand = connection.CreateCommand();
                    checkCommand.CommandText = "SELECT COUNT(*) FROM UltimoInicio;";
                    long cantidad = (long)checkCommand.ExecuteScalar();

                    // ✅ Si no hay datos, insertar un usuario por defecto
                    if (cantidad == 0)
                    {
                        var insertCommand = connection.CreateCommand();
                        insertCommand.CommandText = @"
                    INSERT INTO UltimoInicio (
                        Tipo_Perfil_Usuario_Ultimo_Inicio,
                        Nombre_Usuario_Ultimo_Inicio,
                        Usuario_Ultimo_Inicio,
                        Cedula_Usuario_Ultimo_Inicio
                    )
                    VALUES ('0', 'Admin', 'Admin', '0000000000');
                ";
                        insertCommand.ExecuteNonQuery();

                        Debug.WriteLine("✅ Usuario por defecto insertado en UltimoInicio.");
                    }
                    else
                    {
                        Debug.WriteLine("ℹ️ Ya existe al menos un usuario en UltimoInicio. No se insertó valor por defecto.");
                    }
                }

                Debug.WriteLine("✅ Tabla 'UltimoInicio' creada o ya existente con inserción inicial.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN Create_BD_Ultimo_Inicio_Usuarios]");
                Debug.WriteLine("No se pudo crear la tabla de último inicio o insertar datos.");
                Debug.WriteLine($"Mensaje: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }



        public static void Insertar_Ultimo_Usuario(string tipoPerfil, string nombre, string cedula, string usuario)
        {
            try
            {
                Create_BD_Ultimo_Inicio_Usuarios();
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        // 1. Eliminar todos los registros anteriores
                        var deleteCommand = connection.CreateCommand();
                        deleteCommand.CommandText = "DELETE FROM UltimoInicio;";
                        deleteCommand.ExecuteNonQuery();

                        // 2. Insertar el nuevo usuario
                        var insertCommand = connection.CreateCommand();
                        insertCommand.CommandText = @"
                INSERT INTO UltimoInicio (
                    Tipo_Perfil_Usuario_Ultimo_Inicio,
                    Nombre_Usuario_Ultimo_Inicio,
                    Cedula_Usuario_Ultimo_Inicio,
                    Usuario_Ultimo_Inicio
                )
                VALUES ($tipoPerfil, $nombre, $cedula, $usuario);
                ";

                        insertCommand.Parameters.AddWithValue("$tipoPerfil", tipoPerfil);
                        insertCommand.Parameters.AddWithValue("$nombre", nombre);
                        insertCommand.Parameters.AddWithValue("$cedula", cedula);
                        insertCommand.Parameters.AddWithValue("$usuario", usuario); // 👈 CORREGIDO

                        int filasAfectadas = insertCommand.ExecuteNonQuery();
                        transaction.Commit();

                        if (filasAfectadas > 0)
                        {
                            Debug.WriteLine("✅ Último usuario insertado correctamente (reemplazando anterior).");
                        }
                        else
                        {
                            Debug.WriteLine("⚠️ No se insertó ningún registro en la tabla UltimoInicio.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ Error al insertar en UltimoInicio.");
                Debug.WriteLine($"Mensaje: {ex.Message}");
            }
        }





        public static string Obtener_Tipo_Perfil_Usuario_Ultimo_Inicio()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                SELECT Tipo_Perfil_Usuario_Ultimo_Inicio 
                FROM UltimoInicio 
                ORDER BY ID DESC 
                LIMIT 1;
            ";

                    var resultado = command.ExecuteScalar();
                    return resultado?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ Error al obtener el nombre del último usuario.");
                Debug.WriteLine($"Mensaje: {ex.Message}");
                return string.Empty;
            }
        }
     

        public static string Obtener_Cedula_Ultimo_Usuario()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                SELECT Cedula_Usuario_Ultimo_Inicio 
                FROM UltimoInicio 
                ORDER BY ID DESC 
                LIMIT 1;
            ";

                    var resultado = command.ExecuteScalar();
                    return resultado?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ Error al obtener la cédula del último usuario.");
                Debug.WriteLine($"Mensaje: {ex.Message}");
                return string.Empty;
            }
        }
        public static string Obtener_Nombre_Ultimo_Usuario()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                SELECT Nombre_Usuario_Ultimo_Inicio 
                FROM UltimoInicio 
                ORDER BY ID DESC 
                LIMIT 1;
            ";

                    var resultado = command.ExecuteScalar();
                    return resultado?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ Error al obtener la cédula del último usuario.");
                Debug.WriteLine($"Mensaje: {ex.Message}");
                return string.Empty;
            }
        }
        public static string Obtener_Ultimo_Usuario()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                SELECT Usuario_Ultimo_Inicio 
                FROM UltimoInicio 
                ORDER BY ID DESC 
                LIMIT 1;
            ";

                    var resultado = command.ExecuteScalar();
                    return resultado?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ Error al obtener la cédula del último usuario.");
                Debug.WriteLine($"Mensaje: {ex.Message}");
                return string.Empty;
            }
        }



        #endregion
    }
}
