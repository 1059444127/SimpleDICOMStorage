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
using SimpleDICOMStorageService.DICOM;
using SimpleDICOMStorageService.Configuration;
using SimpleDICOMStorageService.Storage;
using log4net;
using System.Reflection;

namespace SimpleDICOMStorageService
{
    /// <summary>
    /// Listener.
    /// </summary>
    public class StorageListener : IDicomServerHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(StorageListener));

        /// <summary>
        /// Create a storage listener using a configuration.
        /// </summary>
        /// <param name="config"></param>
        public StorageListener(ListenerConfigurationInstance config)
        {
            Configuration = config;
        }

        /// <summary>
        /// Create a storage listener using a server and association parameters.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="assoc"></param>
        private StorageListener(
            ListenerConfigurationInstance config,
            DicomServer server,
            ServerAssociationParameters assoc)
        {
            Configuration = config;
            Configuration.Statistics =
                new Statistics(new AssociationStatisticsRecorder(server));
        }

        #region Private Methods

        #endregion

        #region Public Methods

        /// <summary>
        /// Listening is to commence.
        /// </summary>
        public void StartListening()
        {
            log.Info(
                string.Format("Listening on port {0} for AE {1}",
                    this.Configuration.Port,
                    this.Configuration.AETitle));

            AssocParameters =
                new ServerAssociationParameters(
                    this.Configuration.AETitle,
                    new IPEndPoint(IPAddress.Any, this.Configuration.Port));

            PresentationContextManager.AddPresentationContexts(AssocParameters, this.Configuration.SOPClassRules);

            // Kicks off into the thread pool.
            log.Info(
                string.Format("Launching into dicom server on AE {0} port {1}",
                AssocParameters.CalledAE, AssocParameters.LocalEndPoint.Port));

            DicomServer.StartListening(
                AssocParameters,
                (server, assoc) => new StorageListener(Configuration, server, assoc)
                );

            log.Info("Dicom server has been launched");
        }

        /// <summary>
        /// Listening is complete.
        /// </summary>
        /// <param name="port"></param>
        public void StopListening(int port)
        {
            log.Info(
                 string.Format("Stop Listening on port {0} for AE {1}",
                     this.Configuration.Port,
                     this.Configuration.AETitle));

            DicomServer.StopListening(AssocParameters);
        }

        #endregion

        #region IDicomServerHandler Members

        /// <summary>
        /// Handle an incoming request
        /// </summary>
        /// <param name="server"></param>
        /// <param name="association"></param>
        /// <param name="presentationID"></param>
        /// <param name="message"></param>
        void IDicomServerHandler.OnReceiveRequestMessage(DicomServer server, ServerAssociationParameters association, byte presentationID, DicomMessage message)
        {
            log.Info("Message received ... ");

            try
            {
                log.Info(
                    string.Format("Request recieved with command field {0} from ae {1}",
                    message.CommandField,
                    message.CommandSet[DicomTags.SourceApplicationEntityTitle].ToString()));

                if (message.CommandField == DicomCommandField.CEchoRequest)
                {
                    server.SendCEchoResponse(presentationID, message.MessageId, DicomStatuses.Success);
                    return;
                }

                String studyInstanceUid = null;
                String seriesInstanceUid = null;
                DicomUid sopInstanceUid = null;
                DicomUid sopClassUid = null;
                String patientName = null;

                bool ok = message.DataSet[DicomTags.SopInstanceUid].TryGetUid(0, out sopInstanceUid);
                if (ok) ok = message.DataSet[DicomTags.SeriesInstanceUid].TryGetString(0, out seriesInstanceUid);
                if (ok) ok = message.DataSet[DicomTags.StudyInstanceUid].TryGetString(0, out studyInstanceUid);
                if (ok) ok = message.DataSet[DicomTags.PatientsName].TryGetString(0, out patientName);
                if (ok) ok = message.DataSet[DicomTags.SopClassUid].TryGetUid(0, out sopClassUid);

                if (!ok)
                {
                    log.Info("Unable to retrieve UIDs from request message, sending failure status.");

                    server.SendCStoreResponse(presentationID, message.MessageId, sopInstanceUid.UID,
                        DicomStatuses.ProcessingFailure);
                    return;
                }

                TransferSyntax syntax = association.GetPresentationContext(presentationID).AcceptedTransferSyntax;

                if (!CacheManager.StorageSpaceAvailable(message, Configuration.Root, Configuration.MaxDiskPercentage))
                {
                    log.Error("Insufficient space to store.");

                    server.SendCStoreResponse(presentationID, message.MessageId, sopInstanceUid.UID,
                        DicomStatuses.StorageStorageOutOfResources);
                    return;
                }

                log.Info("Adding to cache");
                CacheManager.AddToCache(
                    Configuration,
                    association,
                    message,
                    syntax,
                    studyInstanceUid,
                    seriesInstanceUid,
                    sopInstanceUid.UID,
                    patientName);

                log.Info(string.Format("Received SOP Instance: {0} for patient {1} in syntax {2}", sopInstanceUid,
                             patientName, syntax.Name));

                server.SendCStoreResponse(presentationID, message.MessageId,
                    sopInstanceUid.UID,
                    DicomStatuses.Success);
            }
            catch (Exception ex)
            {
                server.SendCStoreResponse(presentationID, message.MessageId, message.AffectedSopInstanceUid,
                    DicomStatuses.ProcessingFailure);
                log.Error("Exception thrown " + ex.ToString());
            }
        }


        /// <summary>
        /// Association request
        /// </summary>
        /// <param name="server"></param>
        /// <param name="association"></param>
        void IDicomServerHandler.OnReceiveAssociateRequest(DicomServer server, ServerAssociationParameters association)
        {
            log.Debug("Associate request received");
            server.SendAssociateAccept(association);
        }

        /// <summary>
        /// Response received? Huh?
        /// </summary>
        /// <param name="server"></param>
        /// <param name="association"></param>
        /// <param name="presentationID"></param>
        /// <param name="message"></param>
        void IDicomServerHandler.OnReceiveResponseMessage(DicomServer server, ServerAssociationParameters association, byte presentationID, DicomMessage message)
        {
            Platform.Log(LogLevel.Error, "Unexpectedly received response mess on server.");

            server.SendAssociateAbort(DicomAbortSource.ServiceUser, DicomAbortReason.UnrecognizedPDU);
        }

        /// <summary>
        /// Association release.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="association"></param>
        void IDicomServerHandler.OnReceiveReleaseRequest(DicomServer server, ServerAssociationParameters association)
        {
            Platform.Log(LogLevel.Info, "Received association release request from  {0}.", association.CallingAE);
            log.Info(string.Format("Received association release request from  {0}.", association.CallingAE));
        }

        /// <summary>
        /// Abort connection
        /// </summary>
        /// <param name="server"></param>
        /// <param name="association"></param>
        /// <param name="source"></param>
        /// <param name="reason"></param>
        void IDicomServerHandler.OnReceiveAbort(DicomServer server, ServerAssociationParameters association, DicomAbortSource source, DicomAbortReason reason)
        {
            Platform.Log(LogLevel.Error, "Unexpected association abort received.");
        }

        /// <summary>
        /// Network error.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="association"></param>
        /// <param name="e"></param>
        void IDicomServerHandler.OnNetworkError(DicomServer server, ServerAssociationParameters association, Exception e)
        {
            Platform.Log(LogLevel.Error, e, "Unexpected network error over association from {0}.", association.CallingAE);
        }

        /// <summary>
        /// Dimse timeout.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="association"></param>
        void IDicomServerHandler.OnDimseTimeout(DicomServer server, ServerAssociationParameters association)
        {
            Platform.Log(LogLevel.Info, "Received DIMSE Timeout, continuing listening for messages");
        }

        /// <summary>
        /// Log assoc stats.
        /// </summary>
        /// <param name="association"></param>
        protected void LogAssociationStatistics(ServerAssociationParameters association)
        {
            Configuration.Statistics.Update(association);
        }

        #endregion

        public ListenerConfigurationInstance Configuration;
        private ServerAssociationParameters AssocParameters;

    }
}