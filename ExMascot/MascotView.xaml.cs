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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace ExMascot
{
    /// <summary>
    /// MascotView.xaml の相互作用ロジック
    /// </summary>
    public partial class MascotView : UserControl
    {
        const int FixingDuration = 200;
        public MascotView()
        {
            InitializeComponent();
            MascotSources.CollectionChanged += MascotSources_CollectionChanged;
        }

        private void MascotSources_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CurrentMascotIndex = 0;
        }

        public ContinuityStoryboard WalkWithJumpAnimate(TimeSpan Duration)
        {
            ContinuityStoryboard csb = new ContinuityStoryboard();
            var begin = (Storyboard)Resources["ReadyWalkWithJump"];
            ((DoubleAnimationUsingKeyFrames)begin.Children[0]).KeyFrames[0].Value = RotateTransform.Angle;
            ((DoubleAnimationUsingKeyFrames)begin.Children[1]).KeyFrames[0].Value = TranslateTransform.X;
            ((DoubleAnimationUsingKeyFrames)begin.Children[2]).KeyFrames[0].Value = TranslateTransform.Y;
            csb.Add(begin);

            var sb = (Storyboard)Resources["WalkWithJump"];
            sb.RepeatBehavior = new RepeatBehavior(Duration);
            csb.Add(sb);

            var end = (Storyboard)Resources["FinishWalkWithJump"];
            csb.Add(end);
            csb.CurrentStoryboardChanging += (sender, e) =>
            {
                if(e.Storyboard == end)
                {
                    ((DoubleAnimationUsingKeyFrames)end.Children[0]).KeyFrames[0].Value = RotateTransform.Angle;
                    ((DoubleAnimationUsingKeyFrames)end.Children[1]).KeyFrames[0].Value = TranslateTransform.X;
                    ((DoubleAnimationUsingKeyFrames)end.Children[2]).KeyFrames[0].Value = TranslateTransform.Y;
                }
            };

            return csb;
        }

        public bool IsEnableRotation { get; set; } = true;

        public MascotCollection MascotSources { get; } = new MascotCollection();

        private int curInd = 0;
        public int CurrentMascotIndex
        {
            get
            {
                if (MascotSources.Count == 0) return -1;
                return curInd;
            }
            set
            {
                if(MascotSources.Count > value && value >= 0)
                {
                    curInd = value;
                    ImageView.Image = MascotSources[curInd];
                }
                else
                {
                    curInd = -1;
                    ImageView.Image = null;
                }
            }
        }

        private void SwitchableImage_Clicked(object sender, EventArgs e)
        {
            if (IsEnableRotation)
            {
                ImageView.Image = NextMascot();
            }
        }

        private ImageSource NextMascot()
        {
            if (MascotSources.Count == 0) return null;
            if(CurrentMascotIndex + 1 >= MascotSources.Count)
            {
                curInd = 0;
            }
            else
            {
                curInd++;
            }
            return MascotSources[curInd];
        }
    }

    public class MascotCollection : ObservableCollection<ImageSource>
    {

    }
}
