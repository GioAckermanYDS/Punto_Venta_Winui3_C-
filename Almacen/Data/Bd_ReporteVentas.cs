using Almacen.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Almacen.Data
{
    public class Bd_ReporteVentas
    {
        private static string connectionString = "Data Source=almacen.db;Version=3;";

        // Obtener resumen de ventas en un rango de fechas
        public static List<ReporteVentas> ObtenerReporteVentas(DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReporteVentas> reportes = new List<ReporteVentas>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT v.FechaVenta, SUM(dv.Cantidad * dv.PrecioUnitario) AS TotalVendido, 
                           COUNT(DISTINCT v.IdVenta) AS NumeroVentas
                    FROM Ventas v
                    JOIN DetalleVentas dv ON v.IdVenta = dv.IdVenta
                    WHERE v.FechaVenta BETWEEN @FechaInicio AND @FechaFin
                    GROUP BY v.FechaVenta"
                ;

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@FechaFin", fechaFin.ToString("yyyy-MM-dd"));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reportes.Add(new ReporteVentas
                            {
                                Fecha = Convert.ToDateTime(reader["FechaVenta"]),
                                TotalVendido = Convert.ToDouble(reader["TotalVendido"]),
                                NumeroVentas = Convert.ToInt32(reader["NumeroVentas"])
                            });
                        }
                    }
                }
            }
            return reportes;
        }

        // Obtener el producto más vendido en un rango de fechas
        public static string ObtenerProductoMasVendido(DateTime fechaInicio, DateTime fechaFin)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT p.Nombre, SUM(dv.Cantidad) AS TotalVendido
                    FROM DetalleVentas dv
                    JOIN Productos p ON dv.IdProducto = p.IdProducto
                    JOIN Ventas v ON dv.IdVenta = v.IdVenta
                    WHERE v.FechaVenta BETWEEN @FechaInicio AND @FechaFin
                    GROUP BY p.Nombre
                    ORDER BY TotalVendido DESC
                    LIMIT 1"
                ;

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@FechaFin", fechaFin.ToString("yyyy-MM-dd"));

                    var result = command.ExecuteScalar();
                    return result as string ?? "Sin datos";

                }
            }
        }
    }
}
