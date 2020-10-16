using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Saber.Infrastructure;
using Saber.Infrastructure.Exception;
using Saber.Infrastructure.Logger;
using Saber.Infrastructure.UserSettings;
using Saber.Plugin;


namespace Saber.Core.Plugin
{
    public static class PluginsLoader
    {
        public const string PATH = "PATH";
        public const string Python = "python";
        public const string PythonExecutable = "pythonw.exe";

        public static List<PluginPair> Plugins(List<PluginMetadata> metadatas, PluginsSettings settings)
        {
            var csharpPlugins = CSharpPlugins(metadatas).ToList();
            var pythonPlugins = PythonPlugins(metadatas, settings.PythonDirectory);
            var executablePlugins = ExecutablePlugins(metadatas);

            var csharpAIOPlugins = CSharpAIOPlugins(settings);

            var plugins = csharpPlugins.Concat(pythonPlugins).Concat(executablePlugins).Concat(csharpAIOPlugins).ToList();
            return plugins;
        }

        /// <summary>
        /// 
        /// all in one 把所有信息打包 成单个 dll 的插件导入，扫描程序根目录
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static IEnumerable<PluginPair> CSharpAIOPlugins(PluginsSettings settings)
        {

            var plugins = new List<PluginPair>();

            var sources = Directory.GetFiles(Constant.ProgramDirectory).Where(o=>  Regex.IsMatch(Path.GetFileName(o), "Saber.Plugin.[-a-zA-Z_0-9.]+.dll"));

            foreach( var source in sources)
            {
                PluginMetadata metadata = null;
                var milliseconds = Stopwatch.Debug($"|PluginsLoader.CSharpAIOPlugins|Constructor init cost for {source}", () =>
                {

                    Assembly assembly;
                    try
                    {
                        assembly = Assembly.Load(AssemblyName.GetAssemblyName(source));
                    }
                    catch (Exception e)
                    {
                        Log.Exception($"|PluginsLoader.CSharpAIOPlugins|Couldn't load assembly for {source}", e);
                        return;
                    }
                    var types = assembly.GetTypes();
                    Type type;
                    try
                    {
                        type = types.First(o => o.IsClass && !o.IsAbstract && o.GetInterfaces().Contains(typeof(IPlugin)));
                    }
                    catch (InvalidOperationException e)
                    {
                        Log.Exception($"|PluginsLoader.CSharpAIOPlugins|Can't find class implement IPlugin for <{source}>", e);
                        return;
                    }
                    IPlugin plugin;
                    try
                    {
                        plugin = (IPlugin)Activator.CreateInstance(type);
                    }
                    catch (Exception e)
                    {
                        Log.Exception($"|PluginsLoader.CSharpAIOPlugins|Can't create instance for <{source}>", e);
                        return;
                    }
                    try
                    {
                        if (plugin.Metadata() == null)
                        {
                            Log.Error($"|PluginsLoader.CSharpAIOPlugins|Metadata is null for <{source}>");
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Exception($"|PluginsLoader.CSharpAIOPlugins|Metadata for <{source}>", e);
                        return;
                    }

                    settings.UpdatePluginSettings(plugin.Metadata());
                    plugin.Metadata().ExecuteFileName = new FileInfo(source).Name;
                    plugin.Metadata().PluginDirectory = Constant.ProgramDirectory;

                    PluginPair pair = new PluginPair
                    {
                        Plugin = plugin,
                        Metadata = plugin.Metadata()
                    };
                    metadata = plugin.Metadata();
                    plugins.Add(pair);
                });

                if (metadata != null)
                {
                    metadata.InitTime += milliseconds;
                }
               
            }

            return plugins;
        }


        public static IEnumerable<PluginPair> CSharpPlugins(List<PluginMetadata> source)
        {
            var plugins = new List<PluginPair>();
            var metadatas = source.Where(o => o.Language.ToUpper() == AllowedLanguage.CSharp);

            foreach (var metadata in metadatas)
            {
                var milliseconds = Stopwatch.Debug($"|PluginsLoader.CSharpPlugins|Constructor init cost for {metadata.Name}", () =>
                {

#if DEBUG
                    var assembly = Assembly.Load(AssemblyName.GetAssemblyName(metadata.ExecuteFilePath));
                    var types = assembly.GetTypes();
                    var type = types.First(o => o.IsClass && !o.IsAbstract && o.GetInterfaces().Contains(typeof(IPlugin)));
                    var plugin = (IPlugin)Activator.CreateInstance(type);
#else
                    Assembly assembly;
                    try
                    {
                        assembly = Assembly.Load(AssemblyName.GetAssemblyName(metadata.ExecuteFilePath));
                    }
                    catch (Exception e)
                    {
                        Log.Exception($"|PluginsLoader.CSharpPlugins|Couldn't load assembly for {metadata.Name}", e);
                        return;
                    }
                    var types = assembly.GetTypes();
                    Type type;
                    try
                    {
                        type = types.First(o => o.IsClass && !o.IsAbstract && o.GetInterfaces().Contains(typeof(IPlugin)));
                    }
                    catch (InvalidOperationException e)
                    {
                        Log.Exception($"|PluginsLoader.CSharpPlugins|Can't find class implement IPlugin for <{metadata.Name}>", e);
                        return;
                    }
                    IPlugin plugin;
                    try
                    {
                        plugin = (IPlugin)Activator.CreateInstance(type);
                    }
                    catch (Exception e)
                    {
                        Log.Exception($"|PluginsLoader.CSharpPlugins|Can't create instance for <{metadata.Name}>", e);
                        return;
                    }
#endif
                    PluginPair pair = new PluginPair
                    {
                        Plugin = plugin,
                        Metadata = metadata
                    };
                    plugins.Add(pair);
                });
                metadata.InitTime += milliseconds;

            }
            return plugins;
        }

        public static IEnumerable<PluginPair> PythonPlugins(List<PluginMetadata> source, string pythonDirecotry)
        {
            var metadatas = source.Where(o => o.Language.ToUpper() == AllowedLanguage.Python);
            string filename;

            if (string.IsNullOrEmpty(pythonDirecotry))
            {
                var paths = Environment.GetEnvironmentVariable(PATH);
                if (paths != null)
                {
                    var pythonPaths = paths.Split(';').Where(p => p.ToLower().Contains(Python));
                    if (pythonPaths.Any())
                    {
                        filename = PythonExecutable;
                    }
                    else
                    {
                        Log.Error("|PluginsLoader.PythonPlugins|Python can't be found in PATH.");
                        return new List<PluginPair>();
                    }
                }
                else
                {
                    Log.Error("|PluginsLoader.PythonPlugins|PATH environment variable is not set.");
                    return new List<PluginPair>();
                }
            }
            else
            {
                var path = Path.Combine(pythonDirecotry, PythonExecutable);
                if (File.Exists(path))
                {
                    filename = path;
                }
                else
                {
                    Log.Error("|PluginsLoader.PythonPlugins|Can't find python executable in <b ");
                    return new List<PluginPair>();
                }
            }
            Constant.PythonPath = filename;
            var plugins = metadatas.Select(metadata => new PluginPair
            {
                Plugin = new PythonPlugin(filename),
                Metadata = metadata
            });
            return plugins;
        }

        public static IEnumerable<PluginPair> ExecutablePlugins(IEnumerable<PluginMetadata> source)
        {
            var metadatas = source.Where(o => o.Language.ToUpper() == AllowedLanguage.Executable);

            var plugins = metadatas.Select(metadata => new PluginPair
            {
                Plugin = new ExecutablePlugin(metadata.ExecuteFilePath),
                Metadata = metadata
            });
            return plugins;
        }

    }
}