using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using StatbookReader.Models;
using StatbookReader.Translators;

using OfficeOpenXml;

namespace StatbookReader
{
    public static class StatbookReader
    {
        public static StatbookModel ReadStatbook(string filePath)
        {
            // make sure file exists
            FileInfo existingFile = new FileInfo(filePath);
            ExcelPackage excelPackage = new ExcelPackage(existingFile);
            // determine version of statbook
            ExcelWorkbook excelWorkbook = excelPackage.Workbook;
            ExcelWorksheet irgf = excelWorkbook.Worksheets["IGRF"];

            // create proper decoder subclass
            ITranslator translator;
            if(irgf == null)
            {
                // old ibrf?
                throw new InvalidDataException("Cannot translate ibrf files");
            }
            else if (irgf.Cells["A7"].Value.ToString() == "Date:")
            {
                if (irgf.Cells["G10"].Value != null && irgf.Cells["G10"].Value.ToString() == "LEAGUE")
                {
                    // January 2018 version
                    translator = new IGRFV3Translator();
                }
                else
                {
                    // April 2015 version
                    translator = new IGRFV2Translator();
                }
            }
            else
            {
                // April 2014 version
                translator = new IGRFV1Translator();
            }
            return translator.Translate(excelWorkbook);
        }
    }
}
