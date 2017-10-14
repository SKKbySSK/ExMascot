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
using System.Windows.Threading;
using ExMascot.Extensions;

namespace ExMascot
{
    /// <summary>
    /// DesktopMascot.xaml の相互作用ロジック
    /// </summary>
    public partial class DesktopMascot : Window
    {
        bool locked = false;
        bool capturing = false;
        Profile prof = null;
        string path;

        public DesktopMascot(Profile Profile, string Path)
        {
            InitializeComponent();

            Activated += DesktopMascot_Activated;
            Deactivated += DesktopMascot_Deactivated;

            double mw = 0, mh = 0;
            foreach (string f in Profile.Files)
            {
                BitmapImage bi = new BitmapImage(new Uri(f));
                MascotV.MascotSources.Add(bi);
                mw = bi.Width > mw ? bi.Width : mw;
                mh = bi.Height > mh ? bi.Height : mh;
            }

            Topmost = Profile.TopMost;
            MascotV.IsEnableRotation = Profile.OnClick;

            Left = Profile.X;
            Top = Profile.Y;

            if (Profile.Width <= 0)
                Profile.Width = mw;
            if (Profile.Height <= 0)
                Profile.Height = mh;

            Width = Profile.Width;
            Height = Profile.Height;

            path = Path;
            prof = Profile;
            CloseB.Visibility = Visibility.Hidden;
            LockB.Visibility = Visibility.Hidden;
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

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (CloseB.Visibility == Visibility.Hidden)
                {
                    CloseB.Visibility = Visibility.Visible;
                    LockB.Visibility = Visibility.Visible;
                }
                else
                {
                    CloseB.Visibility = Visibility.Hidden;
                    LockB.Visibility = Visibility.Hidden;
                }
            }
        }

        private void CloseB_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LockB_Click(object sender, RoutedEventArgs e)
        {
            if (locked)
            {
                locked = false;
                ViewParent.MouseLeftButtonDown += Grid_MouseLeftButtonDown;
                ViewParent.MouseLeftButtonUp += Grid_MouseLeftButtonUp;
                ViewParent.MouseMove += Grid_MouseMove;
                MascotV.IsEnableRotation = prof.OnClick;
                LockB.Content = "c";
            }
            else
            {
                locked = true;
                ViewParent.MouseLeftButtonDown -= Grid_MouseLeftButtonDown;
                ViewParent.MouseLeftButtonUp -= Grid_MouseLeftButtonUp;
                ViewParent.MouseMove -= Grid_MouseMove;
                MascotV.IsEnableRotation = false;
                LockB.Content = "d";
            }
        }
    }
}
