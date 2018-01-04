using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleDICOMStorageService.Configuration
{
    public class ModalityRule
    {
        public string Modality { get; set; }

        public string Action { get; set; }

        public string Ratio { get; set; }

        public string OutputTransferSyntax { get; set; }
    }
}
