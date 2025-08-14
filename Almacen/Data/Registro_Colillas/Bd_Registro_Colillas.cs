using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Almacen.Data;
using Almacen.Models;
using AlmacenApp.Models;

using System.IO;
using System.Collections.ObjectModel;

namespace Almacen.Data.Registro_Colillas
{
    class Bd_Registro_Colillas
    {
        private static string dbPath = Path.Combine(A_Ruta_db.Ruta_BD, "BdAlmacen.db");

       

        public static void Create_BD_Registro_Venta()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    // Crear la tabla con el nuevo campo Precio_VentaFin
                    command.CommandText = @"
            CREATE TABLE IF NOT EXISTS RegistroDeFactura (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Fecha_Factura_Venta TEXT NOT NULL,
                Factura_Hecha BLOB NOT NULL, -- Guardará el PDF en formato binario
                Numero_Factura TEXT NOT NULL,
                Precio_VentaFin REAL NOT NULL -- Nuevo campo para el precio final de venta
            );";

                    command.ExecuteNonQuery();
                    Debug.WriteLine("✅ Tabla 'RegistroDeFactura' creada correctamente con el campo Precio_VentaFin.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN Create_BD_Registro_Venta]");
                Debug.WriteLine("No se pudo inicializar la base de datos.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }


        public static List<Modelo_RegistroFactura> Obtener_Registros_De_Factura()
        {
            List<Modelo_RegistroFactura> lista = new List<Modelo_RegistroFactura>();

            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    Debug.WriteLine("✅ Conexión a la base de datos abierta correctamente.");

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                SELECT 
                    ID, 
                    Fecha_Factura_Venta, 
                    Factura_Hecha, 
                    Numero_Factura, 
                    Precio_VentaFin 
                FROM RegistroDeFactura 
                ORDER BY ID DESC;";

                    using (var reader = command.ExecuteReader())
                    {
                        int contador = 0;

                        while (reader.Read())
                        {
                            var registro = new Modelo_RegistroFactura
                            {
                                ID = reader.GetInt32(0),
                                FechaFactura = reader.GetString(1),
                                PDF = (byte[])reader["Factura_Hecha"],
                                NumeroFactura = reader.GetString(3),
                                PrecioVentaFin = reader.GetDouble(4) // 👈 Nuevo campo
                            };

                            lista.Add(registro);
                            contador++;

                            Debug.WriteLine($"📦 Registro #{contador} => ID: {registro.ID}, Fecha: {registro.FechaFactura}, Nº Factura: {registro.NumeroFactura}, Precio: {registro.PrecioVentaFin}, Bytes PDF: {registro.PDF.Length}");
                        }

                        Debug.WriteLine($"✅ Total de registros recuperados: {contador}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN Obtener_Registros_De_Factura]");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }

            return lista;
        }

        public static List<Modelo_RegistroFactura> Obtener_Registros_De_Factura_Resumen_Anual()
        {
            List<Modelo_RegistroFactura> lista = new List<Modelo_RegistroFactura>();

            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    Debug.WriteLine("✅ Conexión a la base de datos abierta correctamente.");

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                SELECT 
                    Fecha_Factura_Venta, 
                    Numero_Factura, 
                    Precio_VentaFin 
                FROM RegistroDeFactura 
                ORDER BY Fecha_Factura_Venta DESC;";

                    using (var reader = command.ExecuteReader())
                    {
                        int contador = 0;

                        while (reader.Read())
                        {
                            var registro = new Modelo_RegistroFactura
                            {
                                FechaFactura = reader.GetString(0),
                                NumeroFactura = reader.GetString(1),
                                PrecioVentaFin = reader.GetDouble(2)
                            };

                            lista.Add(registro);
                            contador++;

                            Debug.WriteLine($"📅 #{contador} => Fecha: {registro.FechaFactura}, Nº Factura: {registro.NumeroFactura}, Precio Total: {registro.PrecioVentaFin}");
                        }

                        Debug.WriteLine($"✅ Total de registros cargados: {contador}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN Obtener_Registros_De_Factura_Resumen_Anual]");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }

            return lista;
        }





        public static void InsertarRegistroFactura(string fechaFactura, byte[] facturaHecha, string numeroFactura, double precioVentaFin)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    command.CommandText = @"
INSERT INTO RegistroDeFactura (Fecha_Factura_Venta, Factura_Hecha, Numero_Factura, Precio_VentaFin)
VALUES (@fecha, @facturaBlob, @numero, @precio);";

                    command.Parameters.AddWithValue("@fecha", fechaFactura);
                    command.Parameters.AddWithValue("@facturaBlob", facturaHecha);
                    command.Parameters.AddWithValue("@numero", numeroFactura);
                    command.Parameters.AddWithValue("@precio", precioVentaFin); // 👈 Nuevo parámetro

                    int rowsAffected = command.ExecuteNonQuery();
                    Debug.WriteLine($"✅ Registro insertado correctamente. Filas afectadas: {rowsAffected}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("-----------------------------------------------------------------");
                Debug.WriteLine("[ERROR EN InsertarRegistroFactura]");
                Debug.WriteLine("No se pudo insertar la factura.");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                Debug.WriteLine("-----------------------------------------------------------------");
            }
        }



        public static string Generar_Numero_Factura()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    // 🟢 Verificar si la tabla existe y crearla si es necesario
                    command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='RegistroDeFactura';";
                    var count = Convert.ToInt32(command.ExecuteScalar());

                    if (count == 0) // 🔥 Si la tabla no existe, crearla
                    {
                        command.CommandText = @"
            CREATE TABLE RegistroDeFactura (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Fecha_Factura_Venta TEXT NOT NULL,
                Factura_Hecha BLOB NOT NULL,
                Numero_Factura TEXT NOT NULL
            );";
                        command.ExecuteNonQuery();
                        Debug.WriteLine("✅ Tabla 'RegistroDeFactura' creada correctamente.");
                    }

                    // 🟢 Obtener el último número de factura
                    command.CommandText = "SELECT Numero_Factura FROM RegistroDeFactura ORDER BY ID DESC LIMIT 1;";
                    var result = command.ExecuteScalar();

                    string prefijo = Perfil_User.Obtener_Prefijo(); // 🔥 Obtener prefijo (ejemplo: "fac")
                    int nuevoNumero = 1; // ✅ Valor por defecto si no hay facturas previas

                    if (result != null && result != DBNull.Value)
                    {
                        string ultimaFactura = result.ToString()!;
                        if (ultimaFactura.StartsWith(prefijo)) // 🔍 Validamos que tenga el prefijo
                        {
                            // 🔥 Extraer el número (ejemplo: "fac-342323" ➝ "342323")
                            string numeroExtraido = ultimaFactura.Replace(prefijo + "-", "");
                            if (int.TryParse(numeroExtraido, out int ultimoNumero))
                            {
                                nuevoNumero = ultimoNumero + 1; // ✅ Incrementar en 1
                            }
                        }
                    }

                    // 🛠 Construir el nuevo número de factura
                    string nuevaFactura = $"{prefijo}-{nuevoNumero}";
                    return nuevaFactura;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ERROR EN Generar_Numero_Factura]");
                Debug.WriteLine($"Mensaje de error: {ex.Message}");
                return "Error";
            }
        }




    }
}
