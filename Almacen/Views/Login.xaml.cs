using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Diagnostics;
using Almacen;
using Almacen.Style.Efectos;
using System.Collections.Generic;
using System.Linq;
using Almacen.Estilos_Configuracion.Estilos;
using Almacen.Models;
using Almacen.Views.View_Almacen;

namespace AlmacenApp.Views
{
    #region Variables globales
  

    #endregion

    public sealed partial class Login : Page
    {
        public Login()
        {
            this.InitializeComponent();          

            if (MainWindow.NavigationViewInstance != null)
            {
                var navigationView = MainWindow.NavigationViewInstance;

               
            }
            else
            {
                Debug.WriteLine("NavigationViewInstance es null.");
            }

           
        }      

    }
}

