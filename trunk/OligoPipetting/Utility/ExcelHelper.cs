using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class ExcelHelper
    {
        public static List<List<string>> ReadExcel(string excelFile)
        {
            Application app = new Application();
            app.Visible = false;
            app.DisplayAlerts = false;

            if (!File.Exists(excelFile))
                throw new Exception("cannot find the excel file");

            int pos = excelFile.IndexOf(".xlsx");
            if (pos == -1)
                throw new Exception("Cannot find xls in file name!");

            Workbook workbook = app.Workbooks.Open(excelFile);
            var sheets = workbook.Worksheets;
            Worksheet worksheet = (Worksheet)sheets.get_Item(1);//读取第一张表
            int rowsCount = worksheet.UsedRange.Rows.Count;
            int colsCount = worksheet.UsedRange.Columns.Count;
            Range c1 = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[2, 1];
            Range c2 = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[rowsCount, colsCount];
            Range rng = (Microsoft.Office.Interop.Excel.Range)worksheet.get_Range(c1, c2);
            object[,] exceldata = (object[,])rng.get_Value(Microsoft.Office.Interop.Excel.XlRangeValueDataType.xlRangeValueDefault);
            List<List<string>> allRowStrs = new List<List<string>>();
            for (int r = 0; r < rowsCount; r++)
            {
                List<string> thisRowStrs = new List<string>();
                if (exceldata[r + 1, 1] == null || string.IsNullOrEmpty(exceldata[r + 1, 1].ToString()))
                    break;
                for (int c = 0; c < colsCount; c++)
                {
                    string content = "";
                    if (exceldata[r + 1, c + 1] != null)
                    {
                        content = exceldata[r + 1, c + 1].ToString();
                    }
                    thisRowStrs.Add(content);
                }
                allRowStrs.Add(thisRowStrs);
            }
            app.Quit();
            Console.WriteLine("read excel successfully!");
            return allRowStrs;

        }
    }
}
