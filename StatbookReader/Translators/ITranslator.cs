using OfficeOpenXml;
using StatbookReader.Models;

namespace StatbookReader.Translators
{
    interface ITranslator
    {
        StatbookModel Translate(ExcelWorkbook workbook);
    }
}
