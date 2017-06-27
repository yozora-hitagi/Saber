using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wox.Plugin.Program.Programs;

namespace Wox.Plugin.Program
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

        private string _text = "";
        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs("Text"));
                }
            }
        }

        

        public PluginInitContext _context { get; set; }


        public void start()
        {
            if (Visibility == System.Windows.Visibility.Visible)
            {
                return;
            }

            Visibility = System.Windows.Visibility.Visible;

            Text=  _context.API.GetTranslation("wox_plugin_program_indexing");

            var task = Task.Run(() =>
            {
                try
                {
                    Infrastructure.Stopwatch.Normal("|Wox.Plugin.Program.Main|Program index cost", index);

                    Text = _context.API.GetTranslation("wox_plugin_program_indexing_ok");
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
                w = Win32.All(Main.Settings());
                Text = string.Format(_context.API.GetTranslation("wox_plugin_program_indexing_complete"), "Win32");
            });
            var t2 = Task.Run(() =>
            {
                if (Environment.OSVersion.Version.Major >=10)
                {
                    u = UWP.All();
                    Text = string.Format(_context.API.GetTranslation("wox_plugin_program_indexing_complete"), "UWP");
                }
            });
            Task.WaitAll(t1, t2);

            Main.updateIndex(w, u);
        }

      
    }
}
