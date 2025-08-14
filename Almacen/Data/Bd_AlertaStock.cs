using Almacen.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Almacen.Data
{
    public static class Bd_AlertaStock
    {
        private static readonly string dbPath = "almacen.db";

        public static void CrearAlerta(AlertaStock alerta)
        {
            try
            {
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = @"
            INSERT INTO AlertaStock (ProductoId, CantidadActual, Umbral, FechaAlerta) 
            VALUES (@ProductoId, @CantidadActual, @Umbral, @FechaAlerta);";

                command.Parameters.AddWithValue("@ProductoId", alerta.ProductoId);
                command.Parameters.AddWithValue("@CantidadActual", alerta.CantidadActual);
                command.Parameters.AddWithValue("@Umbral", alerta.Umbral);
                command.Parameters.AddWithValue("@FechaAlerta", alerta.FechaAlerta.ToString("yyyy-MM-dd HH:mm:ss"));

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, "Error al crear la alerta de stock.");
            }
        }

        public static List<AlertaStock> ObtenerAlertas()
        {
            var alertas = new List<AlertaStock>();

            try
            {
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT ID, ProductoId, CantidadActual, Umbral, FechaAlerta FROM AlertaStock;";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    alertas.Add(new AlertaStock
                    {
                        ID = reader.GetInt32(0),
                        ProductoId = reader.GetInt32(1),
                        CantidadActual = reader.GetInt32(2),
                        Umbral = reader.GetInt32(3),
                        FechaAlerta = DateTime.Parse(reader.GetString(4))
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, "Error al obtener alertas de stock.");
            }

            return alertas;
        }
    }
}
