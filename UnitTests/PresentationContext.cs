using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleDICOMStorageService.Storage;
using SimpleDICOMStorageService.DICOM;
using SimpleDICOMStorageService.Storage.ModalityRules;
using ClearCanvas.Dicom;
using ClearCanvas.Dicom.Network;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class PresentationContext
    {
        [TestMethod]
        public void WildCard()
        {
            var sops = new SimpleDICOMStorageService.Configuration.SOPClassSet()
            {
                Name = "all",
                Rules = new System.Collections.Generic.List<SimpleDICOMStorageService.Configuration.SOPClass>() { 
                        new SimpleDICOMStorageService.Configuration.SOPClass() { 
                            Uid = "*"
                        }
                    }
            };
            
            var result = PresentationContextManager.FindMatchingSOPS(sops);
            var expected = PresentationContextManager.AllSopClasses;
            expected.Add(SopClass.VerificationSopClass);

            Assert.IsTrue(result.TrueForAll(t => expected.Exists(s => s.Name == t.Name)));

        }

        [TestMethod]
        public void SingleCT()
        {
            var sops = new SimpleDICOMStorageService.Configuration.SOPClassSet()
            {
                Name = "all",
                Rules = new System.Collections.Generic.List<SimpleDICOMStorageService.Configuration.SOPClass>() { 
                        new SimpleDICOMStorageService.Configuration.SOPClass() { 
                            Uid = SopClass.CtImageStorageUid
                        }
                    }
            };

            var result = PresentationContextManager.FindMatchingSOPS(sops);
            var expected = new List<SopClass>();
            expected.Add(SopClass.VerificationSopClass);
            expected.Add(SopClass.CtImageStorage);

            Assert.IsTrue(result.TrueForAll(t => expected.Exists(s => s.Name == t.Name)));

        }

        [TestMethod]
        public void PartialWildCard()
        {
            var sops = new SimpleDICOMStorageService.Configuration.SOPClassSet()
            {
                Name = "all",
                Rules = new System.Collections.Generic.List<SimpleDICOMStorageService.Configuration.SOPClass>() { 
                        new SimpleDICOMStorageService.Configuration.SOPClass() { 
                            Uid = "1.2.840.10008.5.1.4.1.1.11.*"
                        }
                    }
            };

            var result = PresentationContextManager.FindMatchingSOPS(sops);
            var expected = new List<SopClass>();
            expected.Add(SopClass.VerificationSopClass);
            expected.Add(SopClass.GrayscaleSoftcopyPresentationStateStorageSopClass);
            expected.Add(SopClass.BlendingSoftcopyPresentationStateStorageSopClass);

            Assert.IsTrue(result.TrueForAll(t => expected.Exists(s => s.Name == t.Name)));

        }

    
    }
}
