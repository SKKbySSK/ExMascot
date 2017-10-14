using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ExMascot.Extensions;

namespace ExMascot
{
    /// <summary>
    /// DesktopMascot.xaml の相互作用ロジック
    /// </summary>
    public partial class DesktopMascot : Window
    {
        bool capturing = false;
        Profile prof = null;
        string path;

        public DesktopMascot(Profile Profile, string Path)
        {
            InitializeComponent();

            Activated += DesktopMascot_Activated;
            Deactivated += DesktopMascot_Deactivated;

            foreach (string f in Profile.Files)
            {
                MascotV.MascotSources.Add(new BitmapImage(new Uri(f)));
            }

            Topmost = Profile.TopMost;
            MascotV.IsEnableRotation = Profile.OnClick;

            Left = Profile.X;
            Top = Profile.Y;
            path = Path;
            prof = Profile;
        }

        private void DesktopMascot_Deactivated(object sender, EventArgs e)
        {
            MascotV.Fade(OpacityProperty, prof.IdleOpacity);
        }

        private void DesktopMascot_Activated(object sender, EventArgs e)
        {
            MascotV.Fade(OpacityProperty, prof.Opacity);
        }

        Point lastPoint;
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lastPoint = e.GetPosition(this);
            capturing = true;
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            capturing = false;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (capturing)
            {
                Point nPoint = e.GetPosition(this);
                Point delta = new Point(nPoint.X - lastPoint.X, nPoint.Y - lastPoint.Y);
                Left += delta.X;
                Top += delta.Y;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            prof.X = Left;
            prof.Y = Top;
            Profile.ExportXML(prof, path);
        }
    }
}
