using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saber.Plugin.Program.Programs;

namespace Saber.Plugin.Program
{
    public class Indexing : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private System.Windows.Visibility _visibility = System.Windows.Visibility.Hidden;
        public System.Windows.Visibility Visibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                _visibility = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs("Visibility"));
                }
            }
        }

        private List<string> _text = new List<string>();
        public string Text
        {
            get
            {
                if (_text.Count > 0)
                    return _text.Last();
                else
                    return "";
            }

            set
            {
                _text.Add(value);
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs("Text"));
                    this.PropertyChanged.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs("Texts"));
                }
            }
        }

        public string Texts
        {
            get
            {
                string str = "";
                foreach(string s in _text)
                {
                    str += s + "\r\n";
                }
                return str;
            }
        }

        

        public PluginInitContext _context { get; set; }


        public void start()
        {
            if (Visibility == System.Windows.Visibility.Visible)
            {
                return;
            }

            _text.Clear();

            Visibility = System.Windows.Visibility.Visible;

            Text= "索引中";

            var task = Task.Run(() =>
            {
                try
                {
                    Infrastructure.Stopwatch.Normal("|Wox.Plugin.Program.Main|Program index cost", index);

                    Text = "索引成功";

                    _context.API.ShowMsg("索引成功", "Finish Indexing!",_context.CurrentPluginMetadata.IcoPath);
                }
                catch (Exception e)
                {
                    Text = e.Message;
                }
                Visibility = System.Windows.Visibility.Hidden;

            });

        }

        private void index()
        {
            Win32[] w = { };
            UWP.Application[] u = { };
            var t1 = Task.Run(() =>
            {
                w = index_win32(Main.Settings());
                Text = string.Format("完成{0}索引", "Win32");
            });
            var t2 = Task.Run(() =>
            {
                if (Environment.OSVersion.Version.Major >=10)
                {
                    u = UWP.All();
                    Text = string.Format("完成{0}索引", "UWP");
                }
            });
            Task.WaitAll(t1, t2);

            Main.updateIndex(w, u);
        }


        private Win32[] index_win32(Settings settings)
        {
            ParallelQuery<Win32> programs = new List<Win32>().AsParallel();
            if (settings.EnableRegistrySource)
            {
                var appPaths = Win32.AppPathsPrograms(settings.ProgramSuffixes);
                programs = programs.Concat(appPaths);
                Text = "完成注册表索引";
            }
            if (settings.EnableStartMenuSource)
            {
                var startMenu =Win32.StartMenuPrograms(settings.ProgramSuffixes);
                programs = programs.Concat(startMenu);
                Text = "完成开始菜单索引";
            }
            var unregistered = Win32.UnregisteredPrograms(settings.ProgramSources, settings.ProgramSuffixes,this);

            programs = programs.Concat(unregistered);
            //.Select(ScoreFilter);
            return programs.ToArray();
        }


    }
}
