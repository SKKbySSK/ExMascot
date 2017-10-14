using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace ExMascot.Extensions
{
    static class UIElementEx
    {
        public static void Fade(this UIElement Element, DependencyProperty Property, double To, double Duration = 200)
        {
            DoubleAnimation da = new DoubleAnimation(Element.Opacity, To, new Duration(TimeSpan.FromMilliseconds(Duration)));
            Element.BeginAnimation(Property, da);
        }
    }
}
