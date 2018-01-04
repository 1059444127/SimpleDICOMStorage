using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleDICOMStorageService.Configuration
{
    /// <summary>
    /// Information about files to be written by the server.
    /// </summary>
    public class DICOMFile
    {
        /// <summary>
        /// Tag to use for filename
        /// </summary>
        public uint Tag { get; set; }

        /// <summary>
        /// Overwrite an existing file?
        /// </summary>
        public Boolean Overwrite { get; set; }

        /// <summary>
        /// Extension to use. (lots of people like .dcm)
        /// </summary>
        public string Extension { get; set; }
    }
}
