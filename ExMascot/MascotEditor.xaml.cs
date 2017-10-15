using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using OFD = System.Windows.Forms.OpenFileDialog;

namespace ExMascot
{
    /// <summary>
    /// MascotEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class MascotEditor : Window
    {
        MenuItem MascotMenu;
        Lazy<Sound> Player = new Lazy<Sound>();
        public Mascot Mascot { get; }

        public MascotEditor(Mascot Mascot)
        {
            InitializeComponent();
            DataContext = this;
            MascotMenu = MascotV.ContextMenu.Items[0] as MenuItem;

            this.Mascot = Mascot;
            foreach (Voice v in Mascot.Voices)
                Sentences.Add(v);

            MascotV.MascotSources.Add(new BitmapImage(new Uri(Mascot.ImageFilePath)));

            Sentences.CollectionChanged += Sentences_CollectionChanged;
            MascotMenu.ItemsSource = Sentences.Where((v) => v.OnManual).ToList();
        }

        private void Sentences_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MascotMenu.ItemsSource = Sentences.Where((v) => v.OnManual).ToList();
        }

        public ObservableCollection<Voice> Sentences { get; } = new ObservableCollection<Voice>();

        private void AddVocieB_Click(object sender, RoutedEventArgs e)
        {
            Voice v = new Voice();
            Sentences.Add(v);
            VoiceL.SelectedItem = v;
        }

        private void RemVoiceB_Click(object sender, RoutedEventArgs e)
        {
            if (VoiceL.SelectedItem != null)
                Sentences.Remove((Voice)VoiceL.SelectedItem);
        }

        private void VoiceL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VoiceG.DataContext = VoiceL.SelectedItem;
            if (VoiceL.SelectedItem == null)
                VoiceG.IsEnabled = false;
            else
                VoiceG.IsEnabled = true;
        }

        private void RefB_Click(object sender, RoutedEventArgs e)
        {
            OFD ofd = new OFD();
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((Voice)VoiceL.SelectedItem).FilePath = ofd.FileName;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Mascot.Voices = Sentences.ToList();
        }

        private void PlayB_Click(object sender, RoutedEventArgs e)
        {
            PlayVoice((Voice)VoiceL.SelectedItem);
        }

        private void MascotV_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var col = Sentences.Where((v) => v.OnClick).ToList();
            if (col.Count > 0)
            {
                Random rnd = new Random();
                Voice v = col[rnd.Next(0, col.Count)];
                PlayVoice(v);
            }
        }

        void PlayVoice(Voice Voice)
        {
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MascotMenu.ItemsSource = Sentences.Where((v) => v.OnManual).ToList();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MascotMenu.ItemsSource = Sentences.Where((v) => v.OnManual).ToList();
        }

        private void MascotMenu_Click(object sender, RoutedEventArgs e)
        {
            Voice v = (Voice)((MenuItem)e.OriginalSource).Header;
            PlayVoice(v);
        }
    }
}
