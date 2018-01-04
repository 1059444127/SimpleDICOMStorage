using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClearCanvas.Dicom;
using System.IO;
using ClearCanvas.Dicom.Network;
using log4net;
using System.Runtime.InteropServices;

namespace SimpleDICOMStorageService.Storage
{
    public class CacheManager
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(StorageListener));

        /// <summary>
        /// Store an image into the cache.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="association"></param>
        /// <param name="message"></param>
        /// <param name="syntax"></param>
        /// <param name="studyInstanceUid"></param>
        /// <param name="seriesInstanceUid"></param>
        /// <param name="sopInstanceUid"></param>
        /// <param name="patientName"></param>
        public static void AddToCache(
            ListenerConfigurationInstance configuration,
            ServerAssociationParameters association,
            DicomMessage message,
            TransferSyntax syntax,
            string studyInstanceUid,
            string seriesInstanceUid,
            string sopInstanceUid,
            string patientName
            )
        {
            var path = CachePathFactory.CreatePath(configuration.Root, configuration.StorageStrategy, message);
            log.Info(string.Format("Cache destination path {0}", path));

            if (File.Exists(path) && !configuration.StorageStrategy.File.Overwrite)
            {
                log.Info("Cache destination exists and storage strategy says don't overwrite!");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            var dicomFile = new DicomFile(message, path.ToString())
            {
                TransferSyntaxUid = syntax.UidString,
                MediaStorageSopInstanceUid = sopInstanceUid,
                ImplementationClassUid = DicomImplementation.ClassUID.UID,
                ImplementationVersionName = DicomImplementation.Version,
                SourceApplicationEntityTitle = association.CallingAE,
                MediaStorageSopClassUid = message.SopClass.Uid
            };

            // Retrieve a modalityRule action and apply it.
            ModalityRuleFactory.Create(message, configuration).Process(dicomFile);

            log.Info("Saving file");
            dicomFile.Save(DicomWriteOptions.None);

        }

        /// <summary>
        /// Check there is sufficient storage space present to manage the request and 
        /// stay under the high water mark.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="root"></param>
        /// <param name="percentage"></param>
        /// <returns></returns>
        internal static bool StorageSpaceAvailable(DicomMessage message, string root, double percentage)
        {
            var result = false;

            // Call will fail if we don't create the directory.
            if (!Directory.Exists(root))
            {
                log.Info(string.Format("Creating directory {0}", root));
                Directory.CreateDirectory(root);
            }

            ulong availableFree, totalBytes, totalFree;
            if (GetDiskFreeSpaceEx(root, out availableFree, out totalBytes, out totalFree))
            {
                var requestedBytes = message.DataSet[DicomTags.PixelData].StreamLength;

                var pathPercentage = 100.0 - ((availableFree + requestedBytes) / (totalBytes * 1.0)) * 100.0;
                if (pathPercentage < percentage)
                {
                    result = true;
                }

                log.Info(string.Format("Cache area {0} has {1} bytes of which {2} are available ({3}%)",
                    root, totalBytes, availableFree, pathPercentage));

            }

            return result;
        }

        // http://stackoverflow.com/questions/1393711/get-free-disk-space
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
        out ulong lpFreeBytesAvailable,
        out ulong lpTotalNumberOfBytes,
        out ulong lpTotalNumberOfFreeBytes);

        public static bool DriveFreeBytes(string folderName, out ulong freespace)
        {
            freespace = 0;
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName");
            }

            if (!folderName.EndsWith("\\"))
            {
                folderName += '\\';
            }

            ulong free = 0, dummy1 = 0, dummy2 = 0;

            if (GetDiskFreeSpaceEx(folderName, out free, out dummy1, out dummy2))
            {
                freespace = free;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
