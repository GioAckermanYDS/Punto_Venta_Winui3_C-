using Microsoft.UI.Xaml.Documents;
using System;
using System.Diagnostics;

namespace Almacen.Style.Configuraciones
{
    class Barra_Navegacion
    {
        public static event Action<bool>? PaneStateChanged;

        public static bool IsPaneOpen
        {
            get => _isPaneOpen;
            private set
            {
                _isPaneOpen = value;
                PaneStateChanged?.Invoke(value);
            }
        }

        private static bool _isPaneOpen = true;

       
        public static void SetPaneState(bool isOpen)
        {
            IsPaneOpen = isOpen;
           
        }

       
    }
}
