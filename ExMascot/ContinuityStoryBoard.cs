using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace ExMascot
{
    public class StoryboardEventArgs : EventArgs
    {
        public StoryboardEventArgs(Storyboard Storyboard, int Index)
        {
            this.Storyboard = Storyboard;
            this.Index = Index;
        }

        public Storyboard Storyboard { get; }
        public int Index { get; }
    }

    public class ContinuityStoryboard : ObservableCollection<Storyboard>
    {
        int index = 0;
        bool locked = false;

        public event EventHandler<StoryboardEventArgs> CurrentStoryboardChanging;
        public event EventHandler Completed;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (locked)
                throw new Exception("アニメーション中のためコレクションを変更できません");

            base.OnCollectionChanged(e);
        }

        public void BeginAnimation()
        {
            if(Count > 0)
            {
                locked = true;
                index = 0;

                Storyboard sb = this[0];
                sb.Completed += Sb_Completed;
                CurrentStoryboardChanging?.Invoke(this, new StoryboardEventArgs(sb, index));
                sb.Begin();
            }
        }

        private void Sb_Completed(object sender, EventArgs e)
        {
            this[index].Completed -= Sb_Completed;
            index++;

            if (index < Count)
            {
                Storyboard sb = this[index];
                CurrentStoryboardChanging?.Invoke(this, new StoryboardEventArgs(sb, index));
                sb.Completed += Sb_Completed;
                sb.Begin();
            }
            else
            {
                locked = false;
                Completed?.Invoke(this, new EventArgs());
            }
        }

        public void StopAnimation()
        {
            foreach(Storyboard sb in this)
            {
                sb.Completed -= Sb_Completed;
                sb.Stop();
            }

            if (index < Count && Count > 0)
            {
                Completed?.Invoke(this, new EventArgs());
            }
        }
    }
}
