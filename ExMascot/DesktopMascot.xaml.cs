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
using System.Windows.Media.Animation;

namespace ExMascot
{
    /// <summary>
    /// DesktopMascot.xaml の相互作用ロジック
    /// </summary>
    public partial class DesktopMascot : Window
    {
        DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
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
            MascotV.CurrentMascotIndex = Profile.GetCurrentIndex();
            if (MascotV.CurrentMascotIndex < 0)
                Close();

            Topmost = Profile.TopMost;
            MascotV.IsEnableRotation = Profile.OnClick;

            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
            Left = 0;
            Top = 0;

            ViewParent.Margin = new Thickness(Profile.X, Profile.Y, 0, 0);

            if (Profile.Width <= 0)
                Profile.Width = mw;
            if (Profile.Height <= 0)
                Profile.Height = mh;

            ViewParent.Width = Profile.Width;
            ViewParent.Height = Profile.Height;

            path = Path;
            prof = Profile;
            CloseB.Visibility = Visibility.Hidden;
            LockB.Visibility = Visibility.Hidden;

            UpdateLockState();

            timer.Interval = TimeSpan.FromMilliseconds(Profile.Interval);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (prof.WalkAround)
            {
                const double DurationParHandred = 600;

                Random RDeltaX = new Random(Environment.TickCount);
                Random RDeltaY = new Random(Environment.TickCount + 10);

                double Y = RDeltaX.NextDouble() * (SystemParameters.WorkArea.Height - ViewParent.Height);
                double X = RDeltaY.NextDouble() * (SystemParameters.WorkArea.Width - ViewParent.Width);

                double dX = ViewParent.Margin.Left - X, dY = ViewParent.Margin.Top - Y;
                double length = Math.Sqrt((dX * dX) + (dY * dY));
                double duration = length / 100 * DurationParHandred;
                TimeSpan tdur = TimeSpan.FromMilliseconds(duration);

                ContinuityStoryboard csb = MascotV.WalkWithJumpAnimate(tdur);
                csb.CurrentStoryboardChanging += (_, ev) =>
                {
                    if(ev.Index == 1)
                    {
                        ThicknessAnimation ta = new ThicknessAnimation(ViewParent.Margin, new Thickness(X, Y, 0, 0), new Duration(tdur));
                        ta.FillBehavior = FillBehavior.Stop;
                        ta.Completed += (_1, _2) =>
                        {
                            ViewParent.Margin = new Thickness(X, Y, 0, 0);
                        };

                        ViewParent.BeginAnimation(MarginProperty, ta);
                    }
                };
                csb.Completed += (_, _2) =>
                {
                    csb.StopAnimation();
                    timer.Start();
                };
                csb.BeginAnimation();
                timer.Stop();
            }
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
            lastPoint = e.GetPosition(ViewParent);
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
                Point nPoint = e.GetPosition(ViewParent);
                Point delta = new Point(nPoint.X - lastPoint.X, nPoint.Y - lastPoint.Y);
                ViewParent.Margin = new Thickness(ViewParent.Margin.Left + delta.X, ViewParent.Margin.Top + delta.Y, 0, 0);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            prof.X = ViewParent.Margin.Left;
            prof.Y = ViewParent.Margin.Top;
            prof.Width = ViewParent.Width;
            prof.Height = ViewParent.Height;
            prof.Index = MascotV.CurrentMascotIndex;
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
            prof.Locked = !prof.Locked;
            UpdateLockState();
        }

        void UpdateLockState()
        {
            if (prof.Locked)
            {
                MascotV.IsEnableRotation = false;
                LockB.Content = "d";
            }
            else
            {
                MascotV.IsEnableRotation = prof.OnClick;
                LockB.Content = "c";
            }
        }
    }
}
