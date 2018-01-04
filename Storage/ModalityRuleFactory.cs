using ClearCanvas.Dicom;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleDICOMStorageService.Storage
{
    public class ModalityRuleFactory
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(StorageListener));

        /// <summary>
        /// Factory method
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IModalityRule Create(DicomMessage message, ListenerConfigurationInstance config)
        {
            IModalityRule result = new ModalityRules.DefaultModalityRule();

            // Apply the modality rules here.
            var modality = message.MetaInfo[DicomTags.Modality];
            log.Info(string.Format("Creating a modality rule for modality {0}", modality));

            var modalityConfig = config.ModalityRules.ModalityRules.FirstOrDefault(mr => mr.Modality == modality);
            if (modalityConfig == null)
            {
                log.Info("Could not find a matching modality rule ... looking for the 'all' rule");
                modalityConfig = config.ModalityRules.ModalityRules.FirstOrDefault(mr => mr.Modality == "all");
            }
            
            if (modalityConfig != null)
            {
                if (modalityConfig.Action == "compress")
                {
                    result = new ModalityRules.CompressorModalityRule()
                    {
                        Ratio = modalityConfig.Ratio,
                        OutputTransferSyntax = modalityConfig.OutputTransferSyntax
                    };
                }
                else if (modalityConfig.Action == "decompress")
                {
                    result = new ModalityRules.DecompressorModalityRule()
                    {
                        OutputTransferSyntax = modalityConfig.OutputTransferSyntax
                    };
                }
            }

            return result;
        }

    }
}
