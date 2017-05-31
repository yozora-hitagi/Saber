using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Squirrel;
using Wox.Core.Plugin;
using Wox.Core.Resource;
using Wox.Helper;
using Wox.Infrastructure;
using Wox.Infrastructure.Hotkey;
using Wox.Infrastructure.Image;
using Wox.Plugin;
using Wox.ViewModel;

namespace Wox
{
    public class PublicAPIInstance : IPublicAPI
    {
        private readonly SettingWindowViewModel _settingsVM;
        private readonly MainViewModel _mainVM;

        #region Constructor

        public PublicAPIInstance(SettingWindowViewModel settingsVM, MainViewModel mainVM)
        {
            _settingsVM = settingsVM;
            _mainVM = mainVM;
            GlobalHotkey.Instance.hookedKeyboardCallback += KListener_hookedKeyboardCallback;
            WebRequest.RegisterPrefix("data", new DataWebRequestFactory());

        }

        #endregion

        #region Public API

        public void ChangeQuery(string query, bool requery = false)
        {
            _mainVM.ChangeQueryText(query);
        }

        public void ChangeQueryText(string query, bool selectAll = false)
        {
            _mainVM.ChangeQueryText(query);
        }

        [Obsolete]
        public void CloseApp()
        {
            Application.Current.MainWindow.Close();
        }

        public void RestarApp()
        {
            //感觉这个设置 可见 是多余的
            // _mainVM.MainWindowVisibility = Visibility.Hidden;

            // we must manually save
            // UpdateManager.RestartApp() will call Environment.Exit(0)
            // which will cause ungraceful exit

            //Application.Current.MainWindow.Close() 触发 MainWindow 的 closing 事件， 那里有 _mainVM.Save()
            //_mainVM.Save();

            _settingsVM.Save();
            PluginManager.Save();
            ImageLoader.Save();
            Alphabet.Save();



            System.Windows.Forms.Application.ExitThread();
            System.Windows.Forms.Application.Exit();

            Application.Current.MainWindow.Close();

            System.Windows.Forms.Application.Restart();
            System.Diagnostics.Process.GetCurrentProcess().Kill();

            //System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);  //重新开启当前程序
            //Environment.Exit(0);//关闭当前程序
            //System.Windows.Forms.Application.Restart();

            //UpdateManager.RestartApp();
        }

        [Obsolete]
        public void HideApp()
        {
            _mainVM.MainWindowVisibility = Visibility.Hidden;
        }

        [Obsolete]
        public void ShowApp()
        {
            _mainVM.MainWindowVisibility = Visibility.Visible;
        }

        public void ShowMsg(string title, string subTitle = "", string iconPath = "")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var m = new Msg { Owner = Application.Current.MainWindow };
                m.Show(title, subTitle, iconPath);
            });
        }

        public void OpenSettingDialog()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SettingWindow sw = SingletonWindowOpener.Open<SettingWindow>(this, _settingsVM);
            });
        }

        public void StartLoadingBar()
        {
            _mainVM.ProgressBarVisibility = Visibility.Visible;
        }

        public void StopLoadingBar()
        {
            _mainVM.ProgressBarVisibility = Visibility.Collapsed;
        }

        public void InstallPlugin(string path)
        {
            Application.Current.Dispatcher.Invoke(() => PluginManager.InstallPlugin(path));
        }

        public string GetTranslation(string key)
        {
            return InternationalizationManager.Instance.GetTranslation(key);
        }

        public List<PluginPair> GetAllPlugins()
        {
            return PluginManager.AllPlugins.ToList();
        }

        public event WoxGlobalKeyboardEventHandler GlobalKeyboardEvent;

        [Obsolete("This will be removed in Wox 1.3")]
        public void PushResults(Query query, PluginMetadata plugin, List<Result> results)
        {
            results.ForEach(o =>
            {
                o.PluginDirectory = plugin.PluginDirectory;
                o.PluginID = plugin.ID;
                o.OriginQuery = query;
            });
            Task.Run(() =>
            {
                _mainVM.UpdateResultView(results, plugin, query);
            });
        }

        #endregion

        #region Private Methods

        private bool KListener_hookedKeyboardCallback(KeyEvent keyevent, int vkcode, SpecialKeyState state)
        {
            if (GlobalKeyboardEvent != null)
            {
                return GlobalKeyboardEvent((int)keyevent, vkcode, state);
            }
            return true;
        }
        #endregion
    }
}
