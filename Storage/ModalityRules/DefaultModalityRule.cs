using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleDICOMStorageService.Storage.ModalityRules
{
    public class DefaultModalityRule : IModalityRule
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(StorageListener));

        /// <summary>
        /// Process the rule and side effect the DicomFile
        /// </summary>
        /// <param name="file"></param>
        public void Process(ClearCanvas.Dicom.DicomFile file)
        {
            // the default rule .. does .. nothing
            log.Info("Default modality rule .. no action taken ..");
        }
    }
}
