using ClearCanvas.Dicom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleDICOMStorageService.Storage
{
    public interface IModalityRule
    {
        void Process(DicomFile file);
    }
}
