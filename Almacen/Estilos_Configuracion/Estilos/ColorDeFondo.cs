using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;

namespace Almacen.Estilos_Configuracion.Estilos
{
    public class ColorDeFondo : INotifyPropertyChanged
    {
        public enum Tema
        {
            Claro,
            Oscuro,
            Sepia // 🔹 Se agrega el nuevo tema
        }

        private static ColorDeFondo _instancia = new ColorDeFondo();
        public static ColorDeFondo Instancia => _instancia;

        private Tema _temaActual = Tema.Claro;
        public Tema TemaActual
        {
            get => _temaActual;
            set
            {
                _temaActual = value;
                OnPropertyChanged(nameof(TemaActual));
                CambiarTema(_temaActual);
            }
        }

        private SolidColorBrush _fondoPrimario = new SolidColorBrush(Microsoft.UI.Colors.Black);
        public SolidColorBrush FondoPrimario
        {
            get => _fondoPrimario;
            set
            {
                _fondoPrimario = value;
                OnPropertyChanged(nameof(FondoPrimario));
            }
        }

        private SolidColorBrush _fondoSecundario = new SolidColorBrush(Microsoft.UI.Colors.White);
        public SolidColorBrush FondoSecundario
        {
            get => _fondoSecundario;
            set
            {
                _fondoSecundario = value;
                OnPropertyChanged(nameof(FondoSecundario));
            }
        }

        private SolidColorBrush _fondoTersario = new SolidColorBrush(Microsoft.UI.Colors.White);
        public SolidColorBrush FondoTersario
        {
            get => _fondoTersario;
            set
            {
                _fondoTersario = value;
                OnPropertyChanged(nameof(FondoTersario));
            }
        }

        private SolidColorBrush _fondoDetalles = new SolidColorBrush(Microsoft.UI.Colors.White);
        public SolidColorBrush FondoDetalles
        {
            get => _fondoDetalles;
            set
            {
                _fondoDetalles = value;
                OnPropertyChanged(nameof(FondoDetalles));
            }
        }

        private SolidColorBrush _colorDeLetra = new SolidColorBrush(Microsoft.UI.Colors.Black);
        public SolidColorBrush ColorDeLetra
        {
            get => _colorDeLetra;
            set
            {
                _colorDeLetra = value;
                OnPropertyChanged(nameof(ColorDeLetra));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void CambiarTema(Tema nuevoTema)
        {
            if (nuevoTema == Tema.Oscuro)
            {
               
                FondoPrimario = new SolidColorBrush(Microsoft.UI.Colors.Black);
                FondoSecundario = new SolidColorBrush(ColorHelper.FromArgb(255, 37, 37, 37));
                ColorDeLetra = new SolidColorBrush(Microsoft.UI.Colors.White);
                FondoTersario = new SolidColorBrush(ColorHelper.FromArgb(255, 15, 15, 15));
                FondoDetalles = new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
            else if (nuevoTema == Tema.Sepia) // 🔹 Nuevo tema Sepia
            {
                
                FondoPrimario = new SolidColorBrush(ColorHelper.FromArgb(255, 112, 66, 20)); // Marrón sepia
                FondoSecundario = new SolidColorBrush(ColorHelper.FromArgb(255, 245, 222, 179)); // Beige claro
                ColorDeLetra = new SolidColorBrush(Microsoft.UI.Colors.White);
                FondoTersario = new SolidColorBrush(ColorHelper.FromArgb(255, 120, 77, 34));
                FondoDetalles = new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
            else // Tema Claro
            {
               
                FondoPrimario = new SolidColorBrush(ColorHelper.FromArgb(255, 156, 156, 156));
                FondoSecundario = new SolidColorBrush(ColorHelper.FromArgb(255, 215, 216, 218)); // Beige claro
                ColorDeLetra = new SolidColorBrush(Microsoft.UI.Colors.White);
                FondoDetalles = new SolidColorBrush(Microsoft.UI.Colors.Red);
                FondoTersario = new SolidColorBrush(ColorHelper.FromArgb(255, 139, 139, 139));


            }

           
        }     
        
    }
}
// rgb(243, 243, 243)

// Se llama  RootGrid.DataContext = ColorDeFondo.Instancia;
// Luego en XAML puedes hacer el binding:
//     Background="{Binding FondoSecundario}"
//     Background="{Binding FondoPrimario}"
//     Background="{Binding FondoTersario}"
//     Background = "{Binding FondoDetalles}"
//     Foreground="{Binding ColorDeLetra}"
//     
//     
//
//     
//
//       
//          Foreground="{Binding Tema.ColorDeLetra}"
//            Background = "{Binding Tema.FondoTersario}"
//           Background = "{Binding Tema.FondoSecundario}"
//           Background = "{Binding Tema.FondoPrimario}"
//           {Binding Tema.FondoTersario}
//
//           {Binding ElementName=RootPage, Path=Tema.FondoPrimario}  
//           {Binding ElementName=RootPage, Path=Tema.ColorDeLetra}
//           
//
//
//
// la otra es  public ColorDeFondo Tema => ColorDeFondo.Instancia; this.DataContext = this; 
