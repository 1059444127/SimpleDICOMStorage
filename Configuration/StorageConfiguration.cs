using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleDICOMStorageService.Configuration
{
    /// <summary>
    /// POCO object for storage config.
    /// </summary>
    public class StorageConfiguration
    {
        /// <summary>
        /// Root - cache manager understands this.
        /// </summary>
        public string StorageRoot { get; set; }

        /// <summary>
        /// Storage technique.  Pass to cache manager to inject the
        /// technique we'd like to use.
        /// </summary>
        public StorageStrategy Strategy { get; set; }
    }
}
