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
        MascotContextMenuManager ContextMenuManager;
        Lazy<Sound> Player = new Lazy<Sound>();
        DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        bool capturing = false;
        Profile prof = null;
        string path;
        bool animating = false;

        public DesktopMascot(Profile Profile, string Path)
        {
            InitializeComponent();

            path = Path;
            prof = Profile;

            Activated += DesktopMascot_Activated;
            Deactivated += DesktopMascot_Deactivated;

            double mw = 0, mh = 0;
            foreach (Mascot f in Profile.Mascots)
            {
                BitmapImage bi = new BitmapImage(new Uri(f.ImageFilePath));
                MascotV.MascotSources.Add(bi);
                mw = bi.Width > mw ? bi.Width : mw;
                mh = bi.Height > mh ? bi.Height : mh;
            }

            MascotV.CurrentMascotIndex = Profile.GetCurrentIndex();
            if (MascotV.CurrentMascotIndex < 0)
                Close();

            Topmost = Profile.TopMost;
            MascotV.IsEnableRotation = !Profile.Locked;

            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
            Left = 0;
            Top = 0;

            ContextMenuManager = new MascotContextMenuManager(this, Profile, Profile.Mascots[MascotV.CurrentMascotIndex]);
            ContextMenuManager.PlayMethod = PlayVoice;
            MascotV.ContextMenu = ContextMenuManager.Menu;

            ViewParent.Margin = new Thickness(Profile.X, Profile.Y, 0, 0);

            if (Profile.Width <= 0)
                Profile.Width = mw;
            if (Profile.Height <= 0)
                Profile.Height = mh;

            ViewParent.Width = Profile.Width;
            ViewParent.Height = Profile.Height;

            timer.Interval = TimeSpan.FromMilliseconds(Profile.Interval);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Player.IsValueCreated)
            {
                if (Player.Value.IsPlaying)
                    return;
            }

            if (prof.Behavior == MascotBehavior.None)
                return;

            animating = true;
            const double DurationParHandred = 600;
            double Y = 0;
            double X = 0;
            if (prof.Behavior == MascotBehavior.Walk)
            {
                Random RDeltaX = new Random(Environment.TickCount);
                Random RDeltaY = new Random(Environment.TickCount + 10);

                Y = RDeltaY.NextDouble() * (SystemParameters.WorkArea.Height - ViewParent.Height);
                X = RDeltaX.NextDouble() * (SystemParameters.WorkArea.Width - ViewParent.Width);
            }
            else if(prof.Behavior == MascotBehavior.Follow)
            {
                const double regionThresh = 50;
                System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;

                if (point.X < 0)
                    point.X = 0;
                else if (SystemParameters.WorkArea.Width <= point.X + ViewParent.Width)
                    point.X = (int)(SystemParameters.WorkArea.Width - ViewParent.Width);

                if (point.Y < 0)
                    point.Y = 0;
                else if (SystemParameters.WorkArea.Height <= point.Y + ViewParent.Height)
                    point.Y = (int)(SystemParameters.WorkArea.Height - ViewParent.Height);

                Thickness region = new Thickness(ViewParent.Margin.Left - regionThresh, ViewParent.Margin.Top - regionThresh,
                    ViewParent.Margin.Left + ViewParent.Width + regionThresh, ViewParent.Margin.Top + ViewParent.Height + regionThresh);

                if (IsInsideOfRegion(point.X, point.Y, region))
                    return;

                X = point.X;
                Y = point.Y;
            }

            double dX = ViewParent.Margin.Left - X, dY = ViewParent.Margin.Top - Y;
            double length = Math.Sqrt((dX * dX) + (dY * dY));
            double duration = length / 100 * DurationParHandred;
            TimeSpan tdur = TimeSpan.FromMilliseconds(duration);

            ContinuityStoryboard csb = MascotV.WalkWithJumpAnimate(tdur);
            csb.CurrentStoryboardChanging += (_, ev) =>
            {
                if (ev.Index == 1)
                {
                    ThicknessAnimation ta = new ThicknessAnimation(ViewParent.Margin, new Thickness(X, Y, 0, 0), new Duration(tdur))
                    {
                        FillBehavior = FillBehavior.Stop
                    };
                    ta.Completed += (_1, _2) =>
                    {
                        ViewParent.Margin = new Thickness(X, Y, 0, 0);
                        animating = false;
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

        bool IsInsideOfRegion(double X, double Y, Thickness Region)
        {
            if(Region.Left <= X && X <= Region.Right)
            {
                if (Region.Top <= Y && Y <= Region.Bottom)
                    return true;
            }
            return false;
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

        private void MascotV_Clicked(object sender, EventArgs e)
        {
            Mascot mascot = prof.Mascots[MascotV.CurrentMascotIndex];
            if (!MascotV.IsEnableRotation)
            {
                var col = mascot.Voices.Where((v) => v.OnClick).ToList();
                if (col.Count > 0)
                {
                    Random rnd = new Random();
                    Voice v = col[rnd.Next(0, col.Count)];
                    PlayVoice(v);
                }
            }
            else
            {
                ContextMenuManager.SetMascot(mascot);
            }
        }

        void PlayVoice(Voice Voice)
        {
            if (animating) return;
            if (!string.IsNullOrEmpty(Voice.FilePath))
            {
                if (System.IO.File.Exists(Voice.FilePath))
                {
                    if (Player.IsValueCreated)
                        Player.Value.Stop();

                    if (Player.Value.ReadyToPlay(Voice.FilePath))
                        Player.Value.PlayFile();
                    else
                        Console.WriteLine($"再生に失敗しました : {Voice.FilePath}");
                }
            }
            MascotV.ShowCallout(Player.Value.Duration, Voice.Sentence);
        }
    }

    class MascotContextMenuManager
    {
        public Action<Voice> PlayMethod { get; set; }

        MenuItem FollowItem = new MenuItem() { Header = "追いかけさせる" };
        MenuItem WalkItem = new MenuItem() { Header = "歩かせる" };
        MenuItem StayItem = new MenuItem() { Header = "止める" };

        MenuItem CloseWindowItem = new MenuItem() { Header = "閉じる" };
        MenuItem LockMascotItem = new MenuItem() { Header = "マスコットを固定", IsCheckable = true };
        public ContextMenu Menu { get; } = new ContextMenu();

        public MascotContextMenuManager(DesktopMascot Window, Profile Profile, Mascot Mascot)
        {
            SetMascot(Mascot);
            InitHandlers(Window, Profile, Mascot);
        }

        public void SetMascot(Mascot Mascot)
        {
            Menu.Items.Clear();
            foreach (Voice v in Mascot.Voices)
            {
                MenuItem mi = new MenuItem() { Header = v.Message };
                mi.Click += (sender, e) =>
                {
                    PlayMethod?.Invoke(v);
                };
                Menu.Items.Add(mi);
            }
            Menu.Items.Add(new Separator());

            Menu.Items.Add(FollowItem);
            Menu.Items.Add(WalkItem);
            Menu.Items.Add(StayItem);

            Menu.Items.Add(new Separator());

            Menu.Items.Add(LockMascotItem);
            Menu.Items.Add(CloseWindowItem);
        }

        void InitHandlers(DesktopMascot Window, Profile Profile, Mascot Mascot)
        {
            FollowItem.Click += (sender, e) => BehaviorChanged(Profile, MascotBehavior.Follow);
            WalkItem.Click += (sender, e) => BehaviorChanged(Profile, MascotBehavior.Walk);
            StayItem.Click += (sender, e) => BehaviorChanged(Profile, MascotBehavior.None);

            FollowItem.IsChecked = Profile.Behavior == MascotBehavior.Follow;
            WalkItem.IsChecked = Profile.Behavior == MascotBehavior.Walk;
            StayItem.IsChecked = Profile.Behavior == MascotBehavior.None;

            {
                LockMascotItem.IsChecked = Profile.Locked;
                CheckableAssociate(LockMascotItem, (c) =>
                {
                    Profile.Locked = c;
                    Window.MascotV.IsEnableRotation = !Profile.Locked;
                });
            }
            {
                CloseWindowItem.Click += (sender, e) => Window.Close();
            }
        }

        void BehaviorChanged(Profile Profile, MascotBehavior Behavior)
        {
            FollowItem.IsChecked = Behavior == MascotBehavior.Follow;
            WalkItem.IsChecked = Behavior == MascotBehavior.Walk;
            StayItem.IsChecked = Behavior == MascotBehavior.None;
            Profile.Behavior = Behavior;
            
        }

        void CheckableAssociate(MenuItem Item, Action<bool> OnCheckStateChanged)
        {
            Item.Checked += (sender, e) => OnCheckStateChanged?.Invoke(true);
            Item.Unchecked += (sender, e) => OnCheckStateChanged?.Invoke(false);
        }
    }
}
