using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace SimpleDICOMStorageService
{
    public partial class SimpleDICOMStorage : ServiceBase
    {
        StorageListener listener;
        private static readonly ILog log = LogManager.GetLogger(typeof(SimpleDICOMStorage));

        public SimpleDICOMStorage()
        {
            XmlConfigurator.Configure();

            log.Info("Configured logging system");
            try 
            {
 
                InitializeComponent();
                Listeners = new List<StorageListener>();

                var configFile = System.Configuration.ConfigurationManager.AppSettings["ConfigurationFile"];
                log.Info(string.Format("Loading configuration from {0}", configFile));
                
                MainConfiguration.Instance.Load(configFile);

                log.Info("Configuration is loaded!");
            }
            catch (Exception ex)
            {
                log.Error("Error! " + ex.ToString());
                throw;
            }
        }

        protected override void OnStart(string[] args)
        {
            log.Info("Start service");

            foreach (var instance in MainConfiguration.Instance.Listeners)
            {
                listener = new StorageListener(instance);
                listener.StartListening();
                Listeners.Add(listener);
            }
        }

        protected override void OnStop()
        {
            try
            {
                log.Info("Stop service");
                Listeners.ForEach(x => x.StopListening(x.Configuration.Port));
            }
            catch (Exception ex)
            {
                log.Info(ex.ToString());
            }
            }

        private List<StorageListener> Listeners;
    }
}
