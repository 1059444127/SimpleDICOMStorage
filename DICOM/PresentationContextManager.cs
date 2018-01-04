using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using ClearCanvas.Common;
using ClearCanvas.Dicom.Network;
using ClearCanvas.Dicom.Utilities.Statistics;
using ClearCanvas.Dicom;
using System.IO;
using log4net;

namespace SimpleDICOMStorageService.DICOM
{
    public class PresentationContextManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(StorageListener));

        public static List<SopClass> AllSopClasses
        {

            get
            {
                return new List<SopClass>() { 
                SopClass.VerificationSopClass,
                SopClass.GrayscaleSoftcopyPresentationStateStorageSopClass,
                SopClass.KeyObjectSelectionDocumentStorage,
                SopClass.ComprehensiveSrStorage,
                SopClass.BlendingSoftcopyPresentationStateStorageSopClass,
                SopClass.ColonCadSrStorage,
                SopClass.DeformableSpatialRegistrationStorage,
                SopClass.EnhancedSrStorage,
                SopClass.BasicTextSrStorage,
                SopClass.EncapsulatedPdfStorage,
                SopClass.XRayRadiationDoseSrStorage,
                SopClass.ChestCadSrStorage,
                SopClass.EncapsulatedCdaStorage,
                SopClass.MrImageStorage,
                SopClass.CtImageStorage,
                SopClass.SecondaryCaptureImageStorage,
                SopClass.UltrasoundImageStorage,
                SopClass.UltrasoundImageStorageRetired,
                SopClass.UltrasoundMultiFrameImageStorage,
                SopClass.UltrasoundMultiFrameImageStorageRetired,
                SopClass.NuclearMedicineImageStorage,
                SopClass.DigitalIntraOralXRayImageStorageForPresentation,
                SopClass.DigitalIntraOralXRayImageStorageForProcessing,
                SopClass.DigitalMammographyXRayImageStorageForPresentation,
                SopClass.DigitalMammographyXRayImageStorageForProcessing,
                SopClass.DigitalXRayImageStorageForPresentation,
                SopClass.DigitalXRayImageStorageForProcessing,
                SopClass.ComputedRadiographyImageStorage,
                SopClass.OphthalmicPhotography16BitImageStorage,
                SopClass.OphthalmicPhotography8BitImageStorage,
                SopClass.VideoEndoscopicImageStorage,
                SopClass.VideoMicroscopicImageStorage,
                SopClass.VideoPhotographicImageStorage,
                SopClass.VlEndoscopicImageStorage,
                SopClass.VlMicroscopicImageStorage,
                SopClass.VlPhotographicImageStorage,
                SopClass.VlSlideCoordinatesMicroscopicImageStorage,
                SopClass.XRayAngiographicBiPlaneImageStorageRetired,
                SopClass.XRayAngiographicImageStorage,
                SopClass.XRayRadiofluoroscopicImageStorage,
                SopClass.XRay3dAngiographicImageStorage,
                SopClass.XRay3dCraniofacialImageStorage,
                SopClass.OphthalmicTomographyImageStorage,
                SopClass.EnhancedCtImageStorage,
                SopClass.EnhancedMrColorImageStorage,
                SopClass.EnhancedMrImageStorage,
                SopClass.EnhancedPetImageStorage,
                SopClass.BreastTomosynthesisImageStorage
            };
            }
        }

        /// <summary>
        /// Image transfer syntaxes
        /// </summary>
        /// <param name="pcid"></param>
        /// <param name="assoc"></param>
        public static void SetImageTransferSyntaxes(byte pcid, ServerAssociationParameters assoc)
        {
            assoc.AddTransferSyntax(pcid, TransferSyntax.JpegLosslessNonHierarchicalFirstOrderPredictionProcess14SelectionValue1);
            assoc.AddTransferSyntax(pcid, TransferSyntax.RleLossless);
            assoc.AddTransferSyntax(pcid, TransferSyntax.Jpeg2000ImageCompression);
            assoc.AddTransferSyntax(pcid, TransferSyntax.Jpeg2000ImageCompressionLosslessOnly);
            assoc.AddTransferSyntax(pcid, TransferSyntax.JpegBaselineProcess1);
            assoc.AddTransferSyntax(pcid, TransferSyntax.JpegExtendedProcess24);
            assoc.AddTransferSyntax(pcid, TransferSyntax.ExplicitVrLittleEndian);
            assoc.AddTransferSyntax(pcid, TransferSyntax.ImplicitVrLittleEndian);
        }

        /// <summary>
        /// Build a list of matching SOPs.
        /// </summary>
        /// <param name="sops"></param>
        /// <returns></returns>
        public static List<SopClass> FindMatchingSOPS(Configuration.SOPClassSet sops)
        {
            var result = new List<SopClass>();

            sops.Rules.ForEach(
                r =>
                {
                    if (r.Uid == "*")
                    {
                        AllSopClasses.ForEach(ms =>
                        {
                            // Add all rules
                            result.Add(ms);
                        });
                    }
                    else if (r.Uid.Contains("*"))
                    {
                        AllSopClasses
                            .FindAll(sc => sc.Uid.StartsWith(r.Uid.TrimEnd('*')))
                            .ForEach(ms =>
                        {
                            // Add all rules
                            result.Add(ms);
                        });
                    }
                    else
                    {
                        var sop = SopClass.GetSopClass(r.Uid);
                        if (sop != null)
                        {
                            // Add all rules
                            result.Add(sop);
                        }
                        else
                        {
                            log.Error(string.Format("SOP class {0} is not defined!", sop));
                        }
                    }
                });

            return result;
        }

        public static void AddPresentationContexts(ServerAssociationParameters assoc, Configuration.SOPClassSet sops)
        {
            byte pcid = assoc.AddPresentationContext(SopClass.VerificationSopClass);
            assoc.AddTransferSyntax(pcid, TransferSyntax.ExplicitVrLittleEndian);
            assoc.AddTransferSyntax(pcid, TransferSyntax.ImplicitVrLittleEndian);

            FindMatchingSOPS(sops).ForEach(
                s => {
                            pcid = assoc.AddPresentationContext(s);
                            SetImageTransferSyntaxes(pcid, assoc);
                            log.Info(string.Format("Allow SOP class {0}", s.Name));
                }
            );
        }

    }
}
