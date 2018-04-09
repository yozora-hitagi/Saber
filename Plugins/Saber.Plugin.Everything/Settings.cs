using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Saber.Infrastructure.Storage;

namespace Saber.Plugin.Everything
{
    public class Settings
    {
        public List<ContextMenu> ContextMenus = new List<ContextMenu>();

        public int MaxSearchCount { get; set; } = 100;

        public bool UseLocationAsWorkingDir { get; set; } = false;
    }

    public class ContextMenu
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public string Argument { get; set; }
        public string ImagePath { get; set; }
    }
}