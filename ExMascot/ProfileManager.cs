using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace ExMascot
{
    static class ProfileManager
    {
        static ProfileManager()
        {
            RefreshDirectory();
        }

        static void RefreshDirectory()
        {
            Profiles.Clear();
            DamagedProfiles.Clear();
            Directory.CreateDirectory("Profiles");

            string[] files = Directory.GetFiles("Profiles", "*.profile");
            foreach (string file in files)
            {
                try
                {
                    Profiles.Add(Profile.LoadXML(file));
                }
                catch (Exception)
                {
                    DamagedProfiles.Add(file);
                }
            }
        }

        public static void AddProfile(Profile Profile, string FileName)
        {
            Profile.ExportXML(Profile, $"Profiles/{FileName}.profile");
            RefreshDirectory();
        }

        public static ObservableCollection<Profile> Profiles { get; } = new ObservableCollection<Profile>();
        public static List<string> DamagedProfiles { get; } = new List<string>();
    }

    public class Profile
    {
        public string TItle { get; set; }
        public bool TopMost { get; set; } = true;
        public bool OnClick { get; set; } = true;
        public bool OnTime { get; set; } = true;
        public TimeSpan TimeSpan { get; set; } = TimeSpan.FromMinutes(3);
        public double Opacity { get; set; } = 0.8;
        public double IdleOpacity { get; set; } = 1;
        public string[] Files { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool WalkAround { get; set; } = true;
        public double Interval { get; set; } = 5000;
        public bool Locked { get; set; } = false;
        public int Index { get; set; } = -1;

        public int GetCurrentIndex()
        {
            if(Index > 0 && Index < Files.Length)
            {
                return Index;
            }
            else
            {
                if (Files.Length > 0)
                    return 0;
                else
                    return -1;
            }
        }

        public override string ToString()
        {
            return TItle;
        }

        public static void ExportXML(Profile Profile, string FilePath)
        {
            XmlSerializer ser = new XmlSerializer(typeof(Profile));
            using (StreamWriter sw = new StreamWriter(FilePath))
            {
                ser.Serialize(sw, Profile);
            }
        }

        public static Profile LoadXML(string FilePath)
        {
            XmlSerializer ser = new XmlSerializer(typeof(Profile));
            using(StreamReader sr = new StreamReader(FilePath))
            {
                return (Profile)ser.Deserialize(sr);
            }
        }
    }
}
