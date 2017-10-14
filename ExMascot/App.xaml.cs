using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ExMascot
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main(string[] Args)
        {
            App app = new App();
            
            foreach(string arg in Args)
            {
                string larg = arg.ToLower();
                if (larg.EndsWith(".profile"))
                {
                    Profile = Profile.LoadXML(larg);
                    ProfilePath = arg;
                }
            }
            
            app.InitializeComponent();
            app.Run();
        }

        public static Profile Profile { get; private set; }
        public static string ProfilePath { get; private set; }
    }
}
