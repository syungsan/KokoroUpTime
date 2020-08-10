using System;
using System.Collections.Generic;
using System.Text;

using System.Windows;

namespace KokoroUpTime
{
    class DB2Excel
    {
        public static void WriteDB2Excel(string dbPath, string outputPath, string[] userInfos)
        {
            var excel = new ExcelManager();
            excel.Open(outputPath);

            // セル書込(R1C1形式)
            excel.WriteCell(row: 1, col: 2, value: "test");
            // セル書込("A1"形式)
            excel.WriteCell(address: "A1", value: 3.1415926538);

            // 名前を付けて保存する
            if (excel.SaveAs(outputPath) == false)
            {
                MessageBox.Show("ファイルが既に開かれています。\n閉じてから、再試行してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
