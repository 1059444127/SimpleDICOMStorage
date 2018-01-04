using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleDICOMStorageService.Configuration;
using System.Xml.Serialization;
using System.IO;
using log4net;
using System.Globalization;
using System.Reflection;

namespace SimpleDICOMStorageService
{
    public class MainConfiguration
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(MainConfiguration));

        /// <summary>
        /// Default c'tor
        /// </summary>
        private MainConfiguration()
        {
            Listeners = new List<ListenerConfigurationInstance>();
        }

        private static object syncRoot = new Object();
        private static volatile MainConfiguration instance;

        /// <summary>
        /// Config singleton
        /// </summary>
        public static MainConfiguration Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MainConfiguration();
                    }
                }

                return instance;
            }
        }

        public List<SOPClassSet> SopClassRules;
        public List<ListenerConfigurationInstance> Listeners;

        /// <summary>
        /// Deserialize the configuration from an XML document.
        /// </summary>
        /// <param name="filename"></param>
        public void Load(string filename)
        {
            if (!filename.Contains(Path.DirectorySeparatorChar))
            {
                var directory = AppDomain.CurrentDomain.BaseDirectory;
                filename = directory + Path.DirectorySeparatorChar + filename;
            }

            log.Info(string.Format("Load from file {0}", filename));
            var configuration = simpleDICOMStorage.LoadFromFile(filename);

            log.Info("Configuration loaded from file ... mapping ... ");
            // build the storage strategies
            var strategies = configuration.storageStrategies.ConvertAll(
                s => new StorageStrategy()
                {
                    Name = s.name,
                    UseDateSubDirectories =
                        !s.useDateSubDirectoriesSpecified ? false : s.useDateSubDirectories,
                    Directories = s.directories
                        .ConvertAll(
                            d => new DICOMDirectory()
                            {
                                Name = d.name,
                                Default = d.defaultVal,
                                Tag = MakeTag(d.tag)
                            }),
                    File = new DICOMFile()
                    {
                        Tag = s.files.tagSpecified ? MakeTag(s.files.tag) : 0,
                        Overwrite = s.files.overwrite,
                        Extension = s.files.extension
                    }
                });

            var modalityRules = configuration.modalityRuleSets.ConvertAll(
                m => new ModalityRuleSet()
                {
                    Name = m.name,
                    ModalityRules =
                        m.modalityRule.ConvertAll(mr => new ModalityRule()
                        {
                            Action = mr.action,
                            Modality = mr.modality,
                            OutputTransferSyntax = mr.outputTransferSyntax,
                            Ratio = mr.ratio
                        })
                });

            var sopClassRules = configuration.sopClassSets.ConvertAll(
                s => new SOPClassSet()
                {
                    Name = s.name,
                    Rules = s.sopClass.ConvertAll( sr =>
                        new SOPClass() { 
                            Uid = sr.uid
                        })
                });

            // Make the listeners.
            Listeners =
            configuration.listeners.ConvertAll(
                c => new ListenerConfigurationInstance()
                {
                    AETitle = c.aeTitle,
                    Root = c.storage.root,
                    Port = c.port,
                    MaxDiskPercentage = ParsePercentage(c.storage.maxDiskUsage),
                    StorageStrategy = strategies.FirstOrDefault(s => s.Name == c.storage.strategy),
                    ModalityRules = modalityRules.FirstOrDefault(r => r.Name == c.storage.modalityRuleSet),
                    SOPClassRules = sopClassRules.FirstOrDefault(s => s.Name == c.sopClassSet)
                });

            Listeners.ForEach(
                x => 
            log.Info(string.Format("Listener found for {0} on port {1} with Root {2} and max usage of {3}%", 
                    x.AETitle,
                    x.Port,
                    x.Root,
                    x.MaxDiskPercentage)));

        }

        /// <summary>
        /// Parse a percentage.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private double ParsePercentage(string p)
        {
            try
            {
                return double.Parse(p.Replace("%", ""));
            }
            catch (Exception)
            {
                log.Fatal(string.Format("Percentage {0} format is wrong", p));
                throw;
            }
        }

        /// <summary>
        /// Handle a dicom tag and make a clearcanvas uint.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private uint MakeTag(string p)
        {
            uint val = 0;

            if (p.StartsWith("0x"))
            {
                try
                {
                    val = Convert.ToUInt32(p, 16);
                }
                catch (Exception)
                {
                    log.Fatal(string.Format("Hex tags must be of the form 0x00100010 not {0}", p));
                    throw new Exception("Invalid hex tag in configuration file");
                }
            }
            else
            {
                if (!uint.TryParse(p, out val))
                {
                    log.Fatal(
                        string.Format("Integer tags must be of the form 1234567 not {0}", p));
                    throw new Exception("Invalid numeric tag in configuration file");
                }
            }

            return val;
        }
    }
}
