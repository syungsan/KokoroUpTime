using System;
using System.Collections.Generic;
using System.Text;

using OfficeOpenXml;
using System.IO;

namespace ExcelManage
{
    class ExcelManager
    {
        /// <summary>
        /// Excel操作用オブジェクト
        /// </summary>
        private ExcelPackage _excelPackage = null;
        private ExcelWorksheet _excelWorksheet = null;

        /// <summary>
        /// Excelワークブックを開く
        /// </summary>
        public void Open(string filePath)
        {
            try
            {
                // Excelファイルを開く
                var fileInfo = new FileInfo(filePath);
                _excelPackage = new ExcelPackage(fileInfo);

                this.SetSheet();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        // ターゲットのシートをセットする
        public void SetSheet(string sheet="Sheet1")
        {
            // シート名で参照
            _excelWorksheet = _excelPackage.Workbook.Worksheets[sheet];
        }

        /// <summary>
        /// Excelワークブックをファイル名を指定して保存する
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <returns>True:正常終了、False:保存失敗</returns>
        public bool SaveAs(string filename)
        {
            try
            {
                var fileInfo = new FileInfo(filename);
                _excelPackage.SaveAs(fileInfo);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /*
        /// <summary>
        /// セル書込(R1C1形式)
        /// </summary>
        /// <param name="row">row</param>
        /// <param name="col">col</param>
        /// <param name="value">value</param>
        public void WriteCell(int row, int col, object value)
        {
            // 値を書込
            _excelWorksheet.Cells[row, col].Value = value;
        }
        */

        /// <summary>
        /// セル書込("A1"形式)
        /// </summary>
        /// <param name="address">address("A1"形式)</param>
        /// <param name="value">value</param>
        public void WriteCell(string address, object value)
        {
            // 値を書込
            _excelWorksheet.Cells[address].Value = value;
        }

        /// <summary>
        /// セル読取(R1C1形式)
        /// </summary>
        /// <param name="row">row</param>
        /// <param name="col">col</param>
        /// <returns>セルの値</returns>
        public object ReadCell(int row, int col)
        {
            // セル読取
            return _excelWorksheet.Cells[row, col].Value;
        }

        /// <summary>
        /// セル読取("A1"形式)
        /// </summary>
        /// <param name="address">address("A1"形式)</param>
        /// <returns>セルの値</returns>
        public object ReadCell(string address)
        {
            // セル読取
            return _excelWorksheet.Cells[address].Value;
        }

        /// <summary>
        /// レンジ読取(R1C1R2C2形式)
        /// </summary>
        /// <param name="rowFrom">row From</param>
        /// <param name="colFrom">col From</param>
        /// <param name="rowTo">row To</param>
        /// <param name="colTo">col To</param>
        /// <returns>レンジ</returns>
        public object[,] ReadRange(int rowFrom, int colFrom, int rowTo, int colTo)
        {
            // レンジ読取
            return (object[,])_excelWorksheet.Cells[rowFrom, colFrom, rowTo, colTo].Value;
        }

        /// <summary>
        /// レンジ読取("A1:B2"形式)
        /// </summary>
        /// <param name="address">address("A1"形式)</param>
        /// <returns>レンジ</returns>
        public object[,] ReadRange(string addresso)
        {
            // レンジ読取
            return (object[,])_excelWorksheet.Cells[addresso].Value;
        }
    }
}
