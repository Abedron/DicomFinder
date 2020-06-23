using EvilDICOM.Core;
using EvilDICOM.Core.Enums;
using EvilDICOM.Core.Interfaces;
using EvilDICOM.Core.IO.Reading;
using System.Collections.Generic;
using System.Linq;

namespace DicomFinder
{
    public static class DicomFileReader
    {
        public static (string Value, PreambleStatus Status) Read(string filePath, string group, string element)
        {
            TransferSyntax syntax = TransferSyntax.IMPLICIT_VR_LITTLE_ENDIAN;
            PreambleStatus preambleStatus;
            DICOMObject dicomObject = null;
            using (var dicomBinaryReader = new DICOMBinaryReader(filePath))
            {
                preambleStatus = Read(dicomBinaryReader);
                if (preambleStatus == PreambleStatus.Ok || preambleStatus == PreambleStatus.MismatchPreamble128)
                {
                    List<IDICOMElement> metaElements = DICOMFileReader.ReadFileMetadata(dicomBinaryReader, ref syntax);
                    List<IDICOMElement> elements = metaElements.Concat(DICOMElementReader.ReadAllElements(dicomBinaryReader, syntax)).ToList();
                    dicomObject = new DICOMObject(elements);
                }
            }

            IDICOMElement value = dicomObject?.Elements.FirstOrDefault(d => d.Tag.Group == group && d.Tag.Element == element);

            return (value?.DData.ToString(), preambleStatus);
        }

        private static PreambleStatus Read(DICOMBinaryReader dr)
        {
            bool mismatchPreamble123 = false;
            if (dr.StreamLength > 132)
            {
                byte[] nullPreamble = dr.Take(128);
                if (nullPreamble.Any(b => b != 0x00))
                {
                    mismatchPreamble123 = true;
                }
                //READ D I C M
                byte[] dcm = dr.Take(4);
                if (dcm[0] != 'D' || dcm[1] != 'I' || dcm[2] != 'C' || dcm[3] != 'M')
                {
                    return PreambleStatus.WrongPreambleDicm;
                }

                if (mismatchPreamble123)
                {
                    return PreambleStatus.MismatchPreamble128;
                }

                return PreambleStatus.Ok;
            }

            return PreambleStatus.None;
        }
    }
}
