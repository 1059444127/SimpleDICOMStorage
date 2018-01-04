using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleDICOMStorageService.Configuration
{
    public class SOPClassSet
    {
        public string Name { get; set; }
        public List<SOPClass> Rules { get; set; }

    }
}
