using ClearCanvas.Dicom;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDICOMStorageService.Storage.ModalityRules
{
    /// <summary>
    /// Decompress DICOM files
    /// </summary>
    public class DecompressorModalityRule : IModalityRule
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CompressorModalityRule));

        /// <summary>
        /// Run the decompressor.
        /// </summary>
        /// <param name="file"></param>
        public void Process(ClearCanvas.Dicom.DicomFile file)
        {
            var proposedTs = ClearCanvas.Dicom.TransferSyntax.GetTransferSyntax(OutputTransferSyntax);
            var currentTs = file.TransferSyntax;

            ClearCanvas.Dicom.TransferSyntax finalTs = currentTs;

            if (proposedTs != TransferSyntax.ExplicitVrBigEndian &&
                proposedTs != TransferSyntax.ImplicitVrLittleEndian &&
                proposedTs != TransferSyntax.ExplicitVrLittleEndian)
            {
                log.Warn("Decompressor cannot supports target transfer syntax of EVBE, IVLE, EVLE only.  Using IVLE ..");
                finalTs = TransferSyntax.ImplicitVrLittleEndian;
            }
            else
            {
                finalTs = proposedTs;
            }

            log.Info(string.Format("Deompress: Proposed: {0}, current {1}, final {2}", proposedTs, currentTs, finalTs));

            if (currentTs != finalTs)
            {
                file.ChangeTransferSyntax(finalTs);
            }
        }

        public string OutputTransferSyntax { get; set; }

    }
}
