using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleDICOMStorageService.Configuration
{
    public class DICOMDirectory
    {
        public string Name { get; set; }

        public uint Tag { get; set; }

        public string Default { get; set; }

        public string FilePattern { get; set; }

    }
}
