﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Saber.Infrastructure.Storage;

namespace Saber.Plugin.Folder
{
    public class Settings
    {
        [JsonProperty]
        public List<FolderLink> FolderLinks { get; set; } = new List<FolderLink>();
    }
}
