using System;
using System.ComponentModel;

namespace Almacen.Models
{
    public class AlertaStock : INotifyPropertyChanged
    {
        public int ID { get; set; }
        public int ProductoId { get; set; }

        private int _cantidadActual;
        public int CantidadActual
        {
            get => _cantidadActual;
            set
            {
                if (_cantidadActual != value)
                {
                    _cantidadActual = value;
                    OnPropertyChanged(nameof(CantidadActual));
                }
            }
        }

        private int _umbral;
        public int Umbral
        {
            get => _umbral;
            set
            {
                if (_umbral != value)
                {
                    _umbral = value;
                    OnPropertyChanged(nameof(Umbral));
                }
            }
        }

        private DateTime _fechaAlerta;
        public DateTime FechaAlerta
        {
            get => _fechaAlerta;
            set
            {
                if (_fechaAlerta != value)
                {
                    _fechaAlerta = value;
                    OnPropertyChanged(nameof(FechaAlerta));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
