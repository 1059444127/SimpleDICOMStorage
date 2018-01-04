using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleDICOMStorageService.Configuration
{
    /// <summary>
    /// A technique to be used when storing files.
    /// </summary>
    public class StorageStrategy
    {
        /// <summary>
        /// Name of the strategy.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A list of directories indicating the structure down the fs.
        /// </summary>
        public List<DICOMDirectory> Directories { get; set; }

        /// <summary>
        /// Rules for how modalities are handled on this AE.
        /// </summary>
        public List<ModalityRuleSet> ModalityRules { get; set; }

        /// <summary>
        /// Should we maintain a directory for each day's date and then place
        /// the DICOMDirectories under that.
        /// </summary>
        public bool UseDateSubDirectories { get; set; }

        internal DICOMFile File { get; set; }
    }
}
