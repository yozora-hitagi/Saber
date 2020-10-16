using System.Collections.Generic;
using System.Linq;
using Saber.Core.Plugin;

namespace Saber.Plugin.PluginIndicator
{
    public class Main : IPlugin, IPluginI18n
    {
        private PluginInitContext context;


        private static PluginMetadata metadata;

        static Main()
        {
            metadata = new PluginMetadata();
            metadata.ID = "6A122269676E40EB86EB543B945932B9";
            metadata.Name = "Plugin Indicator";
            metadata.ActionKeyword = "*";
            metadata.IcoPath = "Images\\work.png";
            metadata.CanDisabled = false;
        }


        public List<Result> Query(Query query)
        {
            var results = from keyword in PluginManager.NonGlobalPlugins.Keys
                          where keyword.StartsWith(query.Terms[0])
                          let metadata = PluginManager.NonGlobalPlugins[keyword].Metadata
                          let disabled = PluginManager.Settings.Plugins[metadata.ID].Disabled
                          where !disabled
                          select new Result
                          {
                              Title = keyword,
                              SubTitle = $"Activate {metadata.Name} plugin",
                              Score = 100,
                              IcoPath = metadata.IcoPath,
                              Action = c =>
                              {
                                  context.API.ChangeQuery($"{keyword}{Plugin.Query.TermSeperater}");
                                  return false;
                              }
                          };
            return results.ToList();
        }

        public void Init(PluginInitContext context)
        {
            this.context = context;
        }

        public string GetTranslatedPluginTitle()
        {
            return "插件关键词提示";
        }

        public string GetTranslatedPluginDescription()
        {
            return "提供插件关键词搜索提示";
        }

        public PluginMetadata Metadata()
        {
            return metadata;
        }
    }
}
