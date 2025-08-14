using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Almacen.Models
{
    public class Referencia : INotifyPropertyChanged
    {

        public int ID { get; set; }

        private string _codigo = string.Empty;
        public string Codigo
        {
            get => _codigo;
            set => SetProperty(ref _codigo, value);
        }

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
