﻿using System;
using System.Linq;
using Newtonsoft.Json;

namespace Saber.Plugin.Folder
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FolderLink
    {
        [JsonProperty]
        public string Path { get; set; }

        public string Nickname
        {
            get { return Path.Split(new[] { System.IO.Path.DirectorySeparatorChar }, StringSplitOptions.None).Last(); }
        }
    }
}
