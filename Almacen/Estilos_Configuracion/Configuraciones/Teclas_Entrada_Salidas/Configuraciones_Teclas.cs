using Almacen.Models;
using Almacen.Views.View_Almacen;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using Windows.Security.Authentication.Identity.Core;
using Windows.System;

#region Metodo para la entrada de numeros y denegacion de letras 
    namespace Almacen.Estilos_Configuracion.Configuraciones
    {
        // EN EL XAML SE llama la clase >   xmlns:config="using:Almacen.Style.Configuraciones"
    
    
        public static class SoloNumerosBehavior
            {
                public static bool GetAplicar(DependencyObject obj) => (bool)obj.GetValue(AplicarProperty);
                public static void SetAplicar(DependencyObject obj, bool value) => obj.SetValue(AplicarProperty, value);

                public static readonly DependencyProperty AplicarProperty =
                    DependencyProperty.RegisterAttached("Aplicar", typeof(bool), typeof(SoloNumerosBehavior),
                    new PropertyMetadata(false, OnAplicarChanged));

                private static void OnAplicarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
                {
                    if (d is TextBox textBox)
                    {
                        if ((bool)e.NewValue)
                        {
                            textBox.BeforeTextChanging += ValidarTexto;
                        }
                        else
                        {
                            textBox.BeforeTextChanging -= ValidarTexto;
                        }
                    }
                }

                private static void ValidarTexto(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
                {
                    // Permitir solo números y puntos
                    if (!System.Text.RegularExpressions.Regex.IsMatch(args.NewText, @"^[\d.]*$"))
                    {
                        args.Cancel = true;
                    }
                }
            }
        // EN EL XAML SE llama la clase >   xmlns:config="using:Almacen.Style.Configuraciones"
        //
        // se utiliza un
        // <Page.Resources>
        //        < Style TargetType = "TextBox" >
        //            < Setter Property = "config:SoloNumerosBehavior.Aplicar" Value = "True" />
        //        </ Style >
        //    </ Page.Resources >
        //
        //   y se pone en el textbox     config:SoloNumerosBehavior.Aplicar="True"

       

   
    }
#endregion

#region Metodo para la seleccion de entrada teclas arriba y abajo y enter
namespace Almacen.Estilos_Configuracion.Configuraciones.Teclas_Entrada_Salidas
{


    public static class Teclas_Seleccion
    {
        // Propiedad adjunta para habilitar la funcionalidad en un control
        public static bool GetAplicar(DependencyObject obj)
        {
            return (bool)obj.GetValue(AplicarProperty);
        }

        public static void SetAplicar(DependencyObject obj, bool value)
        {
            obj.SetValue(AplicarProperty, value);
        }

        public static readonly DependencyProperty AplicarProperty =
            DependencyProperty.RegisterAttached(
                "Aplicar",
                typeof(bool),
                typeof(Teclas_Seleccion),
                new PropertyMetadata(false));
    }


}
#endregion








