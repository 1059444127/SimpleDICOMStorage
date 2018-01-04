using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClearCanvas.Dicom;
using log4net;

namespace SimpleDICOMStorageService.Storage.ModalityRules
{
    /// <summary>
    /// Compress to a known syntax at a specified ratio.
    /// </summary>
    public class CompressorModalityRule : IModalityRule
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CompressorModalityRule));

        /// <summary>
        /// Process the rule
        /// </summary>
        /// <param name="file"></param>
        public void Process(ClearCanvas.Dicom.DicomFile file)
        {
            var proposedTs = ClearCanvas.Dicom.TransferSyntax.GetTransferSyntax(OutputTransferSyntax);
            var currentTs = file.TransferSyntax;

            var bitsStored = file.DataSet[DicomTags.BitsStored].GetInt16(0, 0);
            var bitsAllocated = file.DataSet[DicomTags.BitsAllocated].GetInt16(0, 0);
            var samplesPerPixel = file.DataSet[DicomTags.SamplesPerPixel].GetInt16(0, 0);
            var photometricInterpretation = file.DataSet[DicomTags.PhotometricInterpretation].GetString(0, "");

            log.Info(string.Format(
                "compress: bits stored {0}, bits allocated {1}, samples per pixel {2}, photometric {3}",
                bitsStored, bitsAllocated, samplesPerPixel, photometricInterpretation));

            log.Info(string.Format("Compress: Proposed: {0}, current {1}", proposedTs, currentTs));
            log.Info("Checking that proposed transfer syntax is consistent with the dataset");

            ClearCanvas.Dicom.TransferSyntax finalTs = currentTs;

            try
            {
                if (proposedTs == TransferSyntax.JpegBaselineProcess1)
                {
                    ValidateJPEGBaselineProcess1(bitsAllocated, bitsStored, samplesPerPixel, photometricInterpretation);
                }
                else if (proposedTs == TransferSyntax.JpegExtendedProcess24)
                {
                    ValidateJPEGExtendedProcess2And4(bitsAllocated, bitsStored, samplesPerPixel, photometricInterpretation);
                }
                else if (proposedTs == TransferSyntax.JpegLosslessNonHierarchicalFirstOrderPredictionProcess14SelectionValue1)
                {
                    ValidateJpegLosslessNonHierarchicalFirstOrderPredictionProcess14SelectionValue1(bitsAllocated, bitsStored, samplesPerPixel, photometricInterpretation);
                }
                else if (proposedTs == TransferSyntax.Jpeg2000ImageCompression)
                {
                    ValidateJpeg2000Lossy(bitsAllocated, bitsStored, samplesPerPixel, photometricInterpretation);
                }
                else if (proposedTs == TransferSyntax.Jpeg2000ImageCompressionLosslessOnly)
                {
                    ValidateJpeg2000Lossless(bitsAllocated, bitsStored, samplesPerPixel, photometricInterpretation);
                }

                // Compression from non-compressed transfer syntaxes 
                if (currentTs == TransferSyntax.ImplicitVrLittleEndian ||
                    currentTs == TransferSyntax.ExplicitVrLittleEndian ||
                    currentTs == TransferSyntax.ExplicitVrBigEndian)
                {
                    // This is fine ... we're compressing something that isn't compressed.
                    finalTs = proposedTs;
                }
                else {

                    // Potentially a problem.  We could be moving from a compressed syntax to 
                    // another compress syntax.  We know the target syntax is legal for this dataset
                    // but the toolkit pukes when moving encapsulated frames.

                    // TO DO: More work needed here.
                    finalTs = proposedTs;
                }

                log.Info(string.Format("Final ts {0}", finalTs));

            }
            catch (NotSupportedException nse)
            {
                log.Error("Compression would be illegal and will NOT be applied. " + nse.ToString());
            }

            if (currentTs != finalTs)
            {
                file.ChangeTransferSyntax(finalTs);
            }
        }

        /// <summary>
        /// Validate JPEG 2000 lossless
        /// </summary>
        /// <param name="bitsAllocated"></param>
        /// <param name="bitsStored"></param>
        /// <param name="samplesPerPixel"></param>
        /// <param name="photometricInterpretation"></param>
        private void ValidateJpeg2000Lossless(short bitsAllocated, short bitsStored, short samplesPerPixel, string photometricInterpretation)
        {
            // see http://estore.merge.com/mergecom3/resources/dicom/javadoc4-7/doc-files/pixel.html
            if (photometricInterpretation == "MONOCHROME1" || photometricInterpretation == "MONOCHROME2")
            {
                if (!
                    (bitsStored == 8 || bitsAllocated == 8 || samplesPerPixel == 1) ||
                    (bitsStored == 10 || bitsAllocated == 16 || samplesPerPixel == 1) ||
                    (bitsStored == 12 || bitsAllocated == 16 || samplesPerPixel == 1) ||
                    (bitsStored == 16 || bitsAllocated == 16 || samplesPerPixel == 1))
                {
                    throw new NotSupportedException("JPEG2000 lossless requires (8,8,1) or (10,16,1) or (12,16,1) or (16,16,1) in Monochrome photometric interpretation");
                }
            }
            else if (photometricInterpretation == "RGB")
            {
                if (bitsStored != 8 || bitsAllocated != 8 || samplesPerPixel != 3)
                {
                    throw new NotSupportedException("JPEG2000 lossless requires 8 bits stored & allocated and one sample per pixel in RGB photometric interpretation");
                }
            }
            else if (photometricInterpretation == "YBR_RCT")
            {
                if (bitsStored != 8 || bitsAllocated != 8 || samplesPerPixel != 3)
                {
                    throw new NotSupportedException("JPEG200 lossless requires 8 bits stored & allocated and 3 samples per pixel in YBR_RCT photometric interpretation");
                }
            }
            else
            {
                throw new NotSupportedException("Unsupported photometric interpretation");
            }
        }

        /// <summary>
        /// Validate JPEG 2000 lossy
        /// </summary>
        /// <param name="bitsAllocated"></param>
        /// <param name="bitsStored"></param>
        /// <param name="samplesPerPixel"></param>
        /// <param name="photometricInterpretation"></param>
        private void ValidateJpeg2000Lossy(short bitsAllocated, short bitsStored, short samplesPerPixel, string photometricInterpretation)
        {
            // see http://estore.merge.com/mergecom3/resources/dicom/javadoc4-7/doc-files/pixel.html
            if (photometricInterpretation == "MONOCHROME1" || photometricInterpretation == "MONOCHROME2")
            {
                if (!
                    (bitsStored == 8 || bitsAllocated == 8 || samplesPerPixel == 1) ||
                    (bitsStored == 10 || bitsAllocated == 16 || samplesPerPixel == 1) ||
                    (bitsStored == 12 || bitsAllocated == 16 || samplesPerPixel == 1) ||
                    (bitsStored == 16 || bitsAllocated == 16 || samplesPerPixel == 1))
                {
                    throw new NotSupportedException("JPEG2000 lossy requires (8,8,1) or (10,16,1) or (12,16,1) or (16,16,1) in Monochrome photometric interpretation");
                }
            }
            else if (photometricInterpretation == "RGB")
            {
                if (bitsStored != 8 || bitsAllocated != 8 || samplesPerPixel != 3)
                {
                    throw new NotSupportedException("JPEG2000 lossy requires 8 bits stored & allocated and one sample per pixel in RGB photometric interpretation");
                }
            }
            else if (photometricInterpretation == "YBR_ICT")
            {
                if (bitsStored != 8 || bitsAllocated != 8 || samplesPerPixel != 3)
                {
                    throw new NotSupportedException("JPEG200 lossy requires 8 bits stored & allocated and 3 samples per pixel in YBR_ICT photometric interpretation");
                }
            }
            else
            {
                throw new NotSupportedException("Unsupported photometric interpretation");
            }
        }

        /// <summary>
        /// Validate lossless process 14
        /// </summary>
        /// <param name="bitsAllocated"></param>
        /// <param name="bitsStored"></param>
        /// <param name="samplesPerPixel"></param>
        /// <param name="photometricInterpretation"></param>
        private void ValidateJpegLosslessNonHierarchicalFirstOrderPredictionProcess14SelectionValue1(short bitsAllocated, short bitsStored, short samplesPerPixel, string photometricInterpretation)
        {
            // see http://estore.merge.com/mergecom3/resources/dicom/javadoc4-7/doc-files/pixel.html
            if (photometricInterpretation == "MONOCHROME1" || photometricInterpretation == "MONOCHROME2")
            {
                if (!
                    ((bitsStored >=2 && bitsStored <=16) &&
                    (bitsAllocated == 8 || bitsAllocated == 16) && 
                    samplesPerPixel == 1))
                {
                    throw new NotSupportedException("JPEG first order prediction requires bits stored in range [2,16] and bits allocated == 8 or 16 and one sample per pixel in Monochrome photometric interpretation");
                }
            }
            else if (photometricInterpretation == "RGB")
            {
                if (bitsStored != 8 || bitsAllocated != 8 || samplesPerPixel != 3)
                {
                    throw new NotSupportedException("JPEG Process 2 and 4 requires 8 bits stored & allocated and one sample per pixel in RGB photometric interpretation");
                }
            }
            else
            {
                throw new NotSupportedException("Unsupported photometric interpretation");
            }
        }

        /// <summary>
        /// Validate extended 2 and 4.
        /// </summary>
        /// <param name="bitsAllocated"></param>
        /// <param name="bitsStored"></param>
        /// <param name="samplesPerPixel"></param>
        /// <param name="photometricInterpretation"></param>
        private void ValidateJPEGExtendedProcess2And4(short bitsAllocated, short bitsStored, short samplesPerPixel, string photometricInterpretation)
        {
            // see http://estore.merge.com/mergecom3/resources/dicom/javadoc4-7/doc-files/pixel.html
            if (photometricInterpretation == "MONOCHROME1" || photometricInterpretation == "MONOCHROME2")
            {
                if (!
                    (bitsStored == 8 || bitsAllocated == 8 || samplesPerPixel == 1) ||
                    (bitsStored == 10 || bitsAllocated == 16 || samplesPerPixel == 1) ||
                    (bitsStored == 12 || bitsAllocated == 16 || samplesPerPixel == 1))
                {
                    throw new NotSupportedException("JPEG Process 2 and 4 requires (8,8,1) or (10,16,1) or (12,16,1) in Monochrome photometric interpretation");
                }
            }
            else if (photometricInterpretation == "RGB")
            {
                if (bitsStored != 8 || bitsAllocated != 8 || samplesPerPixel != 3)
                {
                    throw new NotSupportedException("JPEG Process 2 and 4 requires 8 bits stored & allocated and one sample per pixel in RGB photometric interpretation");
                }
            }
            else if (photometricInterpretation == "YBR_FULL_422" || photometricInterpretation == "YBR_FULL")
            {
                if (bitsStored != 8 || bitsAllocated != 8 || samplesPerPixel != 2)
                {
                    throw new NotSupportedException("JPEG Process 2 and 4 requires 8 bits stored & allocated and one sample per pixel in YBR_FULL or YRB_FULL_422 photometric interpretation");
                }
            }
            else
            {
                throw new NotSupportedException("Unsupported photometric interpretation");
            }
        }

        /// <summary>
        /// Validate JPEG baseline
        /// </summary>
        /// <param name="bitsAllocated"></param>
        /// <param name="bitsStored"></param>
        /// <param name="samplesPerPixel"></param>
        /// <param name="photometricInterpretation"></param>
        private void ValidateJPEGBaselineProcess1(short bitsAllocated, short bitsStored, short samplesPerPixel, string photometricInterpretation)
        {
            // see http://estore.merge.com/mergecom3/resources/dicom/javadoc4-7/doc-files/pixel.html
            if (photometricInterpretation == "MONOCHROME1" || photometricInterpretation == "MONOCHROME2")
            {
                if (bitsStored != 8 || bitsAllocated != 8 || samplesPerPixel != 1)
                {
                    throw new NotSupportedException("JPEG Baseline requires 8 bits stored & allocated and one sample per pixel in Monchrome photometric interpretation");
                }
            }
            else if (photometricInterpretation == "RGB")
            {
                if (bitsStored != 8 || bitsAllocated != 8 || samplesPerPixel != 3)
                {
                    throw new NotSupportedException("JPEG Baseline requires 8 bits stored & allocated and one sample per pixel in RGB photometric interpretation");
                }
            }
            else if (photometricInterpretation == "YBR_FULL_422" || photometricInterpretation == "YBR_FULL")
            {
                if (bitsStored != 8 || bitsAllocated != 8 || samplesPerPixel != 3)
                {
                    throw new NotSupportedException("JPEG Baseline requires 8 bits stored & allocated and one sample per pixel in YBR_FULL or YRB_FULL_422 photometric interpretation");
                }
            }
            else
            {
                throw new NotSupportedException("Unsupported photometric interpretation");
            }
        }

        /// <summary>
        /// Validate and throw on error
        /// </summary>
        private void Validate()
        {
        }

        public string Ratio { get; set; }

        public string OutputTransferSyntax { get; set; }
    }
}
