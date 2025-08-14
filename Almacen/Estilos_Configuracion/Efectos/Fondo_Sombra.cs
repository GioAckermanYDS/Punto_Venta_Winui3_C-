using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Diagnostics;
using System.Numerics;

namespace Almacen.Style.Efectos
{
    public static class Fondo_Sombra
    {
        /// <summary>
        /// Aplica un efecto de sombra personalizada a un elemento UIElement.
        /// </summary>
        /// <param name="element">El elemento al que se le aplicará la sombra.</param>
        public static void Aplicarsombra(FrameworkElement element, double? customHeight = null, Thickness? margin = null)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element), "El elemento no puede ser nulo.");
            }

            // Configurar la alineación del elemento en la parte superior
            element.VerticalAlignment = VerticalAlignment.Top;

            // Obtener el compositor del elemento
            var compositor = ElementCompositionPreview.GetElementVisual(element).Compositor;

            // Crear el efecto de sombra
            var dropShadow = compositor.CreateDropShadow();
            dropShadow.Color = Microsoft.UI.ColorHelper.FromArgb(255, 255, 255, 255); // Color negro puro
            dropShadow.Opacity = 1.0f; // Nivel de opacidad al máximo
            dropShadow.BlurRadius = 40f; // Nivel de desenfoque

            // Aplicar márgenes si están definidos
            Thickness appliedMargin = margin ?? new Thickness(0); // Usar márgenes definidos o 0 por defecto
            dropShadow.Offset = new Vector3((float)appliedMargin.Left, (float)(appliedMargin.Top + 60), 0); // Márgenes superiores y desplazamiento inicial

            // Crear un SpriteVisual para contener la sombra
            var shadowVisual = compositor.CreateSpriteVisual();
            shadowVisual.Shadow = dropShadow;

            // Determinar el tamaño del visual, considerando los márgenes
            double heightToApply = (customHeight ?? element.ActualHeight) - appliedMargin.Top - appliedMargin.Bottom;
            double widthToApply = element.ActualWidth - appliedMargin.Left - appliedMargin.Right;

            if (widthToApply > 0 && heightToApply > 0)
            {
                shadowVisual.Size = new Vector2((float)widthToApply, (float)heightToApply);
                ElementCompositionPreview.SetElementChildVisual(element, shadowVisual);
            }
            else
            {
                element.SizeChanged += (s, e) =>
                {
                    double adjustedWidth = element.ActualWidth - appliedMargin.Left - appliedMargin.Right;
                    double adjustedHeight = (customHeight ?? element.ActualHeight) - appliedMargin.Top - appliedMargin.Bottom;

                    shadowVisual.Size = new Vector2((float)adjustedWidth, (float)adjustedHeight);
                    ElementCompositionPreview.SetElementChildVisual(element, shadowVisual);
                };
            }

           
        }




        // Método para activar la sombra
        // Método para activar la sombra con límite de ancho opcional
        public static void ActivarSombra(FrameworkElement element, double? customHeight = null, Thickness? margin = null)
        {
            // Solo aplica la sombra si no se ha activado previamente
            if (ElementCompositionPreview.GetElementChildVisual(element) == null)
            {
                Aplicarsombra(element, customHeight, margin); // Pasar customHeight y margin al método Aplicarsombra
               
            }
            else
            {
             
            }
        }



        // Método para desactivar la sombra
        public static void DesactivarSombra(FrameworkElement element)
        {
            var childVisual = ElementCompositionPreview.GetElementChildVisual(element);
            if (childVisual != null)
            {
                // Remover la sombra
                ElementCompositionPreview.SetElementChildVisual(element, null);
               
            }
            else
            {
               
            }
        }



    }
}
