using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ExMascot.Extensions;

namespace ExMascot.Controls
{
    class SwitchableImage : Grid
    {
        public event EventHandler Clicked;

        enum VisibledImage
        {
            Image1, Image2
        }
        VisibledImage vi;

        public SwitchableImage()
        {
            Children.Add(Image1);

            Image2.Opacity = 0;
            Children.Add(Image2);
            vi = VisibledImage.Image1;

            MouseLeftButtonDown += Image_MouseLeftButtonDown;
            MouseLeftButtonUp += Image_MouseLeftButtonUp;
        }

        bool flag = false;
        DateTime from;
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CaptureMouse();
            flag = true;
            from = DateTime.Now;
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            if (flag)
            {
                if (DateTime.Now - from <= ClickTimeSpan)
                {
                    Clicked?.Invoke(this, new EventArgs());
                }
            }
            flag = false;
        }

        public TimeSpan ClickTimeSpan { get; set; } = TimeSpan.FromMilliseconds(300);

        Image Image1 = new Image();
        Image Image2 = new Image();

        Image GetCurrentImageView()
        {
            if (vi == VisibledImage.Image1)
                return Image1;
            else
                return Image2;
        }

        public ImageSource Image
        {
            get { return GetCurrentImageView().Source; }
            set
            {
                if(vi == VisibledImage.Image1)
                {
                    Image2.Source = value;
                    Image1.Fade(OpacityProperty, 0);
                    Image2.Fade(OpacityProperty, 1);
                    vi = VisibledImage.Image2;
                }
                else
                {
                    Image1.Source = value;
                    Image2.Fade(OpacityProperty, 0);
                    Image1.Fade(OpacityProperty, 1);
                    vi = VisibledImage.Image1;
                }

                if(value != null)
                {
                    Width = value.Width;
                    Height = value.Height;
                }
            }
        }
    }
}
