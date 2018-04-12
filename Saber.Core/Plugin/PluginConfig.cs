using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Saber.Infrastructure.Exception;
using Saber.Infrastructure.Logger;
using Saber.Plugin;
using Saber.Infrastructure;

namespace Saber.Core.Plugin
{

    internal abstract class PluginConfig
    {
        private const string PluginConfigName = "plugin.json";

        /// <summary>
        /// Parse plugin metadata in giving directories
        /// </summary>
        /// <param name="pluginDirectories"></param>
        /// <returns></returns>
        public static List<PluginMetadata> Parse(string[] pluginDirectories)
        {
            List<PluginMetadata> PluginMetadatas = new List<PluginMetadata>();
           
            var directories = pluginDirectories.SelectMany(Directory.GetDirectories);

             foreach (var directory in directories)
            {
                if (File.Exists(Path.Combine(directory, Constant.DELETE_SIGN)))
                {
                    try
                    {
                        Directory.Delete(directory, true);
                    }
                    catch (Exception e)
                    {
                        Log.Exception($"|PluginConfig.ParsePLuginConfigs|Can't delete <{directory}>", e);
                    }
                }
                else
                {
                    PluginMetadata metadata = GetPluginMetadata(directory);
                    if (metadata != null)
                    {
                        PluginMetadatas.Add(metadata);
                    }
                }
            }
            return PluginMetadatas;
        }


        private static PluginMetadata GetPluginMetadata(string pluginDirectory)
        {
            string configPath = Path.Combine(pluginDirectory, PluginConfigName);
            if (!File.Exists(configPath))
            {
                Log.Error($"|PluginConfig.GetPluginMetadata|Didn't find config file <{configPath}>");
                return null;
            }

            PluginMetadata metadata;
            try
            {
                metadata = JsonConvert.DeserializeObject<PluginMetadata>(File.ReadAllText(configPath));
                metadata.PluginDirectory = pluginDirectory;
                // for plugins which doesn't has ActionKeywords key
                metadata.ActionKeywords = metadata.ActionKeywords ?? new List<string> { metadata.ActionKeyword };
                // for plugin still use old ActionKeyword
                metadata.ActionKeyword = metadata.ActionKeywords?[0];
            }
            catch (Exception e)
            {
                Log.Exception($"|PluginConfig.GetPluginMetadata|invalid json for config <{configPath}>", e);
                return null;
            }


            if (!AllowedLanguage.IsAllowed(metadata.Language))
            {
                Log.Error($"|PluginConfig.GetPluginMetadata|Invalid language <{metadata.Language}> for config <{configPath}>");
                return null;
            }

            if (!File.Exists(metadata.ExecuteFilePath))
            {
                Log.Error($"|PluginConfig.GetPluginMetadata|execute file path didn't exist <{metadata.ExecuteFilePath}> for conifg <{configPath}");
                return null;
            }

            return metadata;
        }
    }
}