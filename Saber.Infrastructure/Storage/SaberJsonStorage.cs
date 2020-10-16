using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saber.Infrastructure.Storage
{
    class SaberJsonStorage<T> : JsonStrorage<T> where T : new()
    {
        public SaberJsonStorage()
        {
            var directoryPath = Path.Combine(Constant.DataDirectory, DirectoryName);
            Helper.ValidateDirectory(directoryPath);

            var filename = typeof(T).Name;
            FilePath = Path.Combine(directoryPath, $"{filename}{FileSuffix}");
        }
    }
}
