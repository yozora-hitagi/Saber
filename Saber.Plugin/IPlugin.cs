using System.Collections.Generic;

namespace Saber.Plugin
{
    public interface IPlugin
    {
        List<Result> Query(Query query);
        void Init(PluginInitContext context);

        PluginMetadata Metadata();
        
    }
}