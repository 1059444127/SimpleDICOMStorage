using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleDICOMStorageService.Storage;
using SimpleDICOMStorageService.DICOM;
using SimpleDICOMStorageService.Storage.ModalityRules;
using ClearCanvas.Dicom;

namespace UnitTests
{
    [TestClass]
    public class Compression
    {
        [TestMethod]
        public void TestMethod1()
        {
            DicomFile file = new DicomFile(@"..\..\..\TestData\XA-MONO2-8-12x-catheter-jpeg70");
            file.Load();

            var rule = new CompressorModalityRule()
            {
                OutputTransferSyntax = TransferSyntax.Jpeg2000ImageCompressionUid,
                Ratio = "5:1"
            };

            rule.Process(file);

            // Taking process 70 to 2000 is not a good  thing.  The file should not change.

    
        }
    }
}
