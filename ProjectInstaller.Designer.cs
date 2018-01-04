namespace SimpleDICOMStorageService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.simpleDICOMStorageProcessorInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.simpleDICOMStorageInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // simpleDICOMStorageProcessorInstaller
            // 
            this.simpleDICOMStorageProcessorInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.simpleDICOMStorageProcessorInstaller.Password = null;
            this.simpleDICOMStorageProcessorInstaller.Username = null;
            // 
            // simpleDICOMStorageInstaller
            // 
            this.simpleDICOMStorageInstaller.Description = "The simple DICOM storage service receives DICOM and writes it to files.";
            this.simpleDICOMStorageInstaller.DisplayName = "Simple DICOM Storage";
            this.simpleDICOMStorageInstaller.ServiceName = "simpleDICOMStorageService";
            this.simpleDICOMStorageInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.simpleDICOMStorageProcessorInstaller,
            this.simpleDICOMStorageInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller simpleDICOMStorageProcessorInstaller;
        private System.ServiceProcess.ServiceInstaller simpleDICOMStorageInstaller;
    }
}