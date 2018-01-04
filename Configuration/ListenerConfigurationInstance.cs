using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleDICOMStorageService.Configuration;

namespace SimpleDICOMStorageService
{
    public class ListenerConfigurationInstance
    {
        public string AETitle { get; set; }
        public int Port { get; set; }
        public string Root { get; set; }
        public double MaxDiskPercentage { get; set; }

        public Statistics Statistics;

        /// <summary>
        /// How images should be stored
        /// </summary>
        public StorageStrategy StorageStrategy { get; set; }

        public ModalityRuleSet ModalityRules { get; set; }


        public SOPClassSet SOPClassRules { get; set; }
    }
}
