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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using OFD = System.Windows.Forms.OpenFileDialog;

namespace ExMascot
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        bool _init = true;
        OFD ofd = new OFD() { Filter = "画像ファイル|*.png;*.jpg;*.jpeg|全てのファイル|*.*", FileName = null };

        public MainWindow()
        {
            InitializeComponent();
            if (App.Profile != null)
            {
                DesktopMascot desktopMascot = new DesktopMascot(App.Profile, App.ProfilePath);
                desktopMascot.Show();
                Close();
            }
            _init = false;
        }

        private void AddB_Click(object sender, RoutedEventArgs e)
        {
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MascotL.Items.Add(ofd.FileName);
                MascotPreview.MascotSources.Add(new BitmapImage(new Uri(ofd.FileName)));
            }
        }

        private void RemB_Click(object sender, RoutedEventArgs e)
        {
            if(MascotL.SelectedIndex > -1)
            {
                MascotPreview.MascotSources.RemoveAt(MascotL.SelectedIndex);
                MascotL.Items.RemoveAt(MascotL.SelectedIndex);
            }
        }

        private void SaveB_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TitleT.Text))
            {
                MessageBox.Show("タイトルを入力してください");
                return;
            }

            Profile prof = new Profile();
            prof.TItle = TitleT.Text;
            prof.Files = MascotL.Items.OfType<string>().ToArray();
            prof.Opacity = OpaS.Value;
            prof.IdleOpacity = IdleOpaS.Value;
            prof.TopMost = IsTopMostC.IsChecked ?? false;

            ProfileManager.AddProfile(prof, prof.TItle);
        }

        private void ProfilesC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ProfilesC.SelectedItem != null)
            {
                Profile prof = (Profile)ProfilesC.SelectedItem;
                TitleT.Text = prof.TItle;

                MascotL.Items.Clear();
                MascotPreview.MascotSources.Clear();
                foreach(string f in prof.Files)
                {
                    MascotL.Items.Add(f);
                    MascotPreview.MascotSources.Add(new BitmapImage(new Uri(f)));
                }

                OpaS.Value = prof.Opacity;
                IdleOpaS.Value = prof.IdleOpacity;
                IsTopMostC.IsChecked = prof.TopMost;
            }
        }

        private void IdleOpaS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_init) return;
            UpdateMascotOpacity();
        }

        private void OpaS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_init) return;
            UpdateMascotOpacity();
        }

        void UpdateMascotOpacity()
        {
            if (IsIdleModeC.IsChecked ?? false)
                MascotPreview.Opacity = IdleOpaS.Value;
            else
                MascotPreview.Opacity = OpaS.Value;
        }

        private void IsIdleModeC_Checked(object sender, RoutedEventArgs e)
        {
            UpdateMascotOpacity();
        }

        private void IsIdleModeC_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateMascotOpacity();
        }
    }
}
