using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;

namespace ClipboardDetector.Providers
{
    public class ProviderExcel
    {
        public static void Load(string path, Dictionary<string, string> map)
        {
            map.Clear();
            var info = new FileInfo(path);
            using (ExcelPackage package = new ExcelPackage(info))
            {
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    int rowCount = sheet.Dimension.End.Row;

                    for (int i = 5; i <= rowCount; i++)
                    {
                        var key = sheet.Cells[i, 1].Value.ToString();
                        if (!string.IsNullOrEmpty(key))
                        {
                            var value = sheet.Cells[i, 2].Value.ToString();
                            map[key] = value;
                        }
                    }
                    break;
                }
            }
        }
    }
}
