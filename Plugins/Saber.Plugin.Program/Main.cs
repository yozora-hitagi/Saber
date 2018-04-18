using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Saber.Infrastructure;
using Saber.Infrastructure.Logger;
using Saber.Infrastructure.Storage;
using Saber.Plugin.Program.Programs;
using Stopwatch = Saber.Infrastructure.Stopwatch;

namespace Saber.Plugin.Program
{
    public class Main : ISettingProvider, IPlugin, IPluginI18n, IContextMenu, ISavable
    {
        private static readonly object IndexLock = new object();
        private static Win32[] _win32s;
        private static UWP.Application[] _uwps;

        private static PluginInitContext _context;

        private static BinaryStorage<Win32[]> _win32Storage;
        private static BinaryStorage<UWP.Application[]> _uwpStorage;
        private static Settings _settings;
        private readonly PluginJsonStorage<Settings> _settingsStorage;

        private static Indexing _indexing;

        public Main()
        {
            _indexing = new Indexing();

            _settingsStorage = new PluginJsonStorage<Settings>();
            _settings = _settingsStorage.Load();

            Stopwatch.Normal("|Wox.Plugin.Program.Main|Preload programs cost", () =>
            {
                _win32Storage = new BinaryStorage<Win32[]>("Win32");
                _win32s = _win32Storage.TryLoad(new Win32[] { });
                _uwpStorage = new BinaryStorage<UWP.Application[]>("UWP");
                _uwps = _uwpStorage.TryLoad(new UWP.Application[] { });
            });
            Log.Info($"|Wox.Plugin.Program.Main|Number of preload win32 programs <{_win32s.Length}>");
            Log.Info($"|Wox.Plugin.Program.Main|Number of preload uwps <{_uwps.Length}>");

        }


        public void Save()
        {
            _settingsStorage.Save();
            _win32Storage.Save(_win32s);
            _uwpStorage.Save(_uwps);
        }

        public List<Result> Query(Query query)
        {
            lock (IndexLock)
            {
                var commands = Commands();
                var results = new List<Result>();
                foreach (var c in commands)
                {
                    var titleScore = StringMatcher.Score(c.Title, query.Search);
                    var subTitleScore = StringMatcher.Score(c.SubTitle, query.Search);
                    var score = Math.Max(titleScore, subTitleScore);
                    if (score > 0)
                    {
                        c.Score = score;
                        results.Add(c);
                    }
                }

                var results1 = _win32s.AsParallel().Select(p => p.Result(query.Search, _context.API));
                var results2 = _uwps.AsParallel().Select(p => p.Result(query.Search, _context.API));

                results.AddRange(results1);
                results.AddRange(results2);

                // var result = results1.Concat(results2).Where(r => r.Score > 0).ToList();

                var result = results.Where(r => r.Score > 0).ToList();
                
                return result;
            }
        }

        private List<Result> Commands()
        {
            var results = new List<Result>();
            results.AddRange(new[]
            {new Result
                {
                    Title = "Reindex",
                    SubTitle = _context.API.GetTranslation("wox_plugin_program_reindex"),
                    IcoPath = "Images\\program.png",
                    Action = c =>
                    {
                        _indexing.start();
                        return true;
                    }
                }
            });

            return results;
        }

        public void Init(PluginInitContext context)
        {
            _context = context;
            _indexing._context = context;
        }

        public static void updateIndex(Win32[] w,UWP.Application[] u)
        {
            lock (IndexLock)
            {
                _win32s = w;
                _uwps = u;
            }
        }

        public static Settings Settings()
        {
            return _settings;
        }

        //public static void IndexPrograms()
        //{
        //    Win32[] w = { };
        //    UWP.Application[] u = { };
        //    var t1 = Task.Run(() =>
        //    {
        //        w = Win32.All(_settings);
        //    });
        //    var t2 = Task.Run(() =>
        //    {
        //        var windows10 = new Version(10, 0);
        //        var support = Environment.OSVersion.Version.Major >= windows10.Major;
        //        if (support)
        //        {
        //            u = UWP.All();
        //        }
        //        else
        //        {
        //            u = new UWP.Application[] { };
        //        }
        //    });
        //    Task.WaitAll(t1, t2);

        //    lock (IndexLock)
        //    {
        //        _win32s = w;
        //        _uwps = u;
        //    }
        //}

        public Control CreateSettingPanel()
        {
            ProgramSetting set = new ProgramSetting(_context, _settings, _indexing);
          
            return set;
        }

        public string GetTranslatedPluginTitle()
        {
            return _context.API.GetTranslation("wox_plugin_program_plugin_name");
        }

        public string GetTranslatedPluginDescription()
        {
            return _context.API.GetTranslation("wox_plugin_program_plugin_description");
        }

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            var program = selectedResult.ContextData as IProgram;
            if (program != null)
            {
                var menus = program.ContextMenus(_context.API);
                return menus;
            }
            else
            {
                return new List<Result>();
            }
        }

        public static bool StartProcess(ProcessStartInfo info)
        {
            bool hide;
            try
            {
                Process.Start(info);
                hide = true;
            }
            catch (Exception)
            {
                var name = "Plugin: Program";
                var message = $"Can't start: {info.FileName}";
                _context.API.ShowMsg(name, message, string.Empty);
                hide = false;
            }
            return hide;
        }
    }
}