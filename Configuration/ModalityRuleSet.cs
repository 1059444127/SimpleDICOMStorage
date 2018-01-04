using SimpleDICOMStorageService.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleDICOMStorageService
{
    public class ModalityRuleSet
    {
        public string Name { get; set; }

        public List<ModalityRule> ModalityRules { get; set; }
    }
}
