using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimpleDICOMStorageService.Storage
{
    public class CachePathFactory
    {
        /// <summary>
        /// Build a path from a strategy and a message.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="storageStrategy"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string CreatePath(string root, Configuration.StorageStrategy storageStrategy, ClearCanvas.Dicom.DicomMessage message)
        {
            string path = root.TrimEnd(Path.DirectorySeparatorChar) + 
                Path.DirectorySeparatorChar;

            if (storageStrategy.UseDateSubDirectories)
            {
                path += DateTime.Now.ToString("yyyyMMdd");
                path += Path.DirectorySeparatorChar;
            }

            var componentList = storageStrategy.Directories.ConvertAll(
                d => message.DataSet[d.Tag].GetString(0, d.Default));

            path += string.Join(Path.DirectorySeparatorChar.ToString(), componentList);
            
            // use a GUID for the filename
            var filename = Guid.NewGuid().ToString();
            if (storageStrategy.File.Tag > 0)
            {
                filename = message.DataSet[storageStrategy.File.Tag].GetString(0, filename);
            }

            path = path + Path.DirectorySeparatorChar + filename + storageStrategy.File.Extension;
            return path;
        }
    }
}
