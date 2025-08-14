using Almacen.Estilos_Configuracion.Estilos;
using AlmacenApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Almacen.Models
{
    public class Producto : INotifyPropertyChanged
    {
        // Propiedades base
        public int ID { get; set; }
        public int CategoriaId { get; set; }

        private string _nombre = string.Empty;
        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        private string _descripcion = string.Empty;
        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        // Lógica de stock
        private int _cantidad;
        public int Cantidad
        {
            get => _cantidad;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "El valor no puede ser negativo.");
                SetProperty(ref _cantidad, value);
            }
        }     



        private int _stockMinimo;
        public int StockMinimo
        {
            get => _stockMinimo;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "El valor no puede ser negativo.");
                SetProperty(ref _stockMinimo, value);
            }
        }

        #region Lógica de precios (numeros)

        private int _precioCompra;
        public int PrecioCompra
        {

            get => _precioCompra;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "El valor no puede ser negativo.");
                if (SetProperty(ref _precioCompra, value))
                {
                    ActualizarTotal(); // Actualiza el campo Total
                    OnPropertyChanged(nameof(PrecioTotal));
                }
            }
        }

        private double _precioFijo;

        public double Precio_Fijo
        {
            get => _precioFijo;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "El valor no puede ser negativo.");
                if (SetProperty(ref _precioFijo, value))
                {
                    ActualizarTotal(); // Actualiza el campo Total
                }
               
            }
            
            
        }

        private double _precioPorcentaje;
        public double Precio_Porcentaje
        {
            get => _precioPorcentaje;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "El valor no puede ser negativo.");
                if (SetProperty(ref _precioPorcentaje, value))
                {
                    ActualizarTotal(); // Actualiza el campo Total
                }
               
            }
        }

        private bool _eleccionCalculo;
        public bool Eleccion_Calculo
        {
            get => _eleccionCalculo;
            set
            {
                if (SetProperty(ref _eleccionCalculo, value))
                {
                    ActualizarTotal(); // Actualiza el campo Total
                    OnPropertyChanged(nameof(PrecioTotal));
                }
            }
        }

        private double _total;
        public double Total
        {
            get => _total;
            private set => SetProperty(ref _total, value); // Solo se asigna internamente
        }

        public int PrecioTotal // Propiedad calculada
        {
            get
            {
                // Realizar el cálculo y redondear al entero más cercano
                return Eleccion_Calculo
                    ? (int)Math.Round(PrecioCompra + Precio_Fijo) // Total con Precio Fijo
                    : (int)Math.Round(PrecioCompra * (1 + (Precio_Porcentaje / 100))); // Total con Porcentaje
            }
        }


        private void ActualizarTotal() // Método centralizado para actualizar el Total
        {
            Total = Eleccion_Calculo
                ? PrecioCompra + Precio_Fijo // Total con Precio Fijo
                : PrecioCompra * (1 + (Precio_Porcentaje / 100)); // Total con Porcentaje

            //Debug.WriteLine($"Total actualizado: {Total} (Elección de cálculo: {(Eleccion_Calculo ? "Fijo" : "Porcentaje")})");
        }

        public double Ganancia
        {
            get
            {
                return Eleccion_Calculo
                    ? Precio_Fijo // Si usa Precio Fijo, refleja su valor directamente
                    : PrecioCompra * (Precio_Porcentaje / 100); // Si usa Porcentaje, calcula con base en el porcentaje
            }
        }

        private void ActualizarGanancia()
        {
            OnPropertyChanged(nameof(Ganancia)); // Notificar que Ganancia ha cambiado
           
        }


        #endregion

        // Lista de referencias
        private List<Referencia> _referencias = new();
        public List<Referencia> Referencias
        {
            get => _referencias;
            set
            {
                if (_referencias != value)
                {
                    _referencias = value ?? new List<Referencia>();
                    OnPropertyChanged(nameof(Referencias));
                }
            }
        }
        public ColorDeFondo Tema => ColorDeFondo.Instancia;


        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
