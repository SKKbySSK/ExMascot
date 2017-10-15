using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Vorbis;

namespace ExMascot
{
    class Sound
    {
        WaveStream wave;

        public event EventHandler Stopped;

        public Sound()
        {
            WavePlayerFactory = () => new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 300);
            WavePlayer = WavePlayerFactory();
        }

        public Sound(Func<IWavePlayer> Factory)
        {
            WavePlayerFactory = Factory;
        }

        public bool IsPlaying
        {
            get
            {
                if (WavePlayer == null) return false;
                else
                {
                    return WavePlayer.PlaybackState == PlaybackState.Playing;
                }
            }
        }

        IWavePlayer _player;
        public IWavePlayer WavePlayer
        {
            get { return _player; }
            set
            {
                if (_player != null)
                {
                    _player.PlaybackStopped -= WavePlayer_PlaybackStopped;
                    _player.Dispose();
                }

                _player = value;
            }
        }

        public Func<IWavePlayer> WavePlayerFactory { get; set; }

        public bool ReadyToPlay(string Path)
        {
            if (WavePlayer == null)
                return false;

            try
            {
                WavePlayer.PlaybackStopped += WavePlayer_PlaybackStopped;

                if (Path.ToLower().EndsWith(".ogg"))
                {
                    wave = new VorbisWaveReader(Path);
                }
                else
                {
                    wave = new AudioFileReader(Path);
                }
                WavePlayer.Init(wave);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                if (wave != null)
                    return wave.TotalTime;
                else
                    return new TimeSpan();
            }
        }

        public void PlayFile()
        {
            WavePlayer?.Play();
        }

        public void Stop()
        {
            if(WavePlayer != null)
            {
                WavePlayer.PlaybackStopped -= WavePlayer_PlaybackStopped;
                WavePlayer.Stop();
                WavePlayer.Dispose();
                WavePlayer = null;

                if (WavePlayerFactory != null)
                    WavePlayer = WavePlayerFactory();
            }

            wave?.Dispose();
            wave = null;
        }

        private void WavePlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            Stop();

            Stopped?.Invoke(this, new EventArgs());
        }
    }
}
