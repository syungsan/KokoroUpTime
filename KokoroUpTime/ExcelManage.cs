using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ExcelManage
{
    class ExcelManager
    {
        /// <summary>
        /// Excel操作用オブジェクト
        /// </summary>
        private ExcelPackage _excelPackage = null;
        private ExcelWorksheet _excelWorksheet = null;
        private ExcelRange _excelRange = null;

        /// <summary>
        /// Excelワークブックを開く
        /// </summary>
        public void Open(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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

        /// <summary>
        /// 画像を貼り付ける
        /// </summary>
        /// <param name="adress"></param>
        /// <param name="bitmapImage"></param>
        public void PastePicture(string adress,string FileName,string ImagePath,int percent)
        {
            var picture =_excelWorksheet.Drawings.AddPicture(FileName, Image.FromFile(ImagePath));

            int column = Regex.Match(adress, @"[a-zA-Z]+").ToString().ToUpper()[0] - 'A';
            int row = int.Parse(Regex.Match(adress, @"\d+").ToString())-1;

            picture.ChangeCellAnchor(eEditAs.OneCell);
            picture.SetPosition(row,5,column,5);
            picture.SetSize(percent);

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
        /// セル書込("A1"形式)
        /// </summary>
        /// <param name="address">address("A1"形式)</param>
        /// <param name="value">value</param>
        public void WriteCell(string address, object value,string asdasd)
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

        /// <summary>
        /// セルの幅を自動で合わせる
        /// </summary>
        /// <param name=""></param>
        /// <param name="adress">幅を合わせるセル名</param>
        public void AutoFitColumns(string adress = "AllRange")
        {
          
            if (adress =="AllRange")
            {
                _excelWorksheet.Cells.AutoFitColumns();
            }
            else
            {
                _excelWorksheet.Cells[adress].AutoFitColumns();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum InsertMode { ShiftRight, ShiftDown, EntierRow, EntierColumn }
        public enum DeleteMode { ShiftLeft, ShiftUp, EntireRow, EntireColumn }

        private const string ERROR_MESSAGE = "データの消失を防ぐため、空白でないセルをワークシートの外に移動することはできません。新しいセルを挿入する別の場所を選択するか、ワークシートの末尾からデータを削除してください。\n\nワークシートの外に移動できるセルにデータがない場合は、どのセルが空白でないと見なされるかをリセットできます。これを行うには、Ctrl+End キーを押してワークシート上の最後の空白でないセルに移動します。次に、このセルと、データの最後の行および列とこのセルの間のセルをすべて削除し、保存します。";

        /// <summary>
        /// セルの挿入
        /// </summary>
        /// <param name="range">セルの範囲</param>
        /// <param name="mode"></param>
        public void InsertCells(string excelRange, InsertMode mode)
        {
            ExcelRange range = _excelWorksheet.Cells[excelRange]; 
            switch (mode)
            {
                // Shift cells right
                case InsertMode.ShiftRight:
                    var endCol1 = _excelWorksheet.Dimension.End.Column + (range.End.Column - range.Start.Column + 1);
                    if (16384 < endCol1)
                    {
                        MessageBox.Show(ERROR_MESSAGE, "Warning");
                        break;
                    }
                    using (var rg = _excelWorksheet.Cells[range.Start.Row, range.Start.Column, range.End.Row, _excelWorksheet.Dimension.End.Column])
                    {
                        rg.Copy(_excelWorksheet.Cells[range.Start.Row, range.End.Column + 1]);
                        range.Clear();
                    }
                    break;

                // Shift cells down
                case InsertMode.ShiftDown:
                    var endRow = _excelWorksheet.Dimension.End.Row + range.End.Row - range.Start.Row + 1;
                    if (1048576 < endRow)
                    {
                        MessageBox.Show(ERROR_MESSAGE, "Warning");
                        break;
                    }
                    using (var rg = _excelWorksheet.Cells[range.Start.Row, range.Start.Column, _excelWorksheet.Dimension.End.Row, range.End.Column])
                    {
                        rg.Copy(_excelWorksheet.Cells[range.End.Row + 1, range.Start.Column]);
                        range.Clear();
                    }
                    break;

                // Entire row
                case InsertMode.EntierRow:
                    var h = range.End.Row - range.Start.Row + 1;
                    var endCol2 = _excelWorksheet.Dimension.End.Column + h;
                    if (16384 < endCol2)
                    {
                        MessageBox.Show(ERROR_MESSAGE, "Warning");
                        break;
                    }
                    _excelWorksheet.InsertRow(range.Start.Row, h);
                    break;

                // Entire column
                case InsertMode.EntierColumn:
                    var w = range.End.Column - range.Start.Column + 1;
                    var endRow2 = _excelWorksheet.Dimension.End.Row + w;
                    if (1048576 < endRow2)
                    {
                        MessageBox.Show(ERROR_MESSAGE, "Warning");
                        break;
                    }
                    _excelWorksheet.InsertColumn(range.Start.Column, w);
                    break;
            }
        }

        public void DrawBorder(string adress, bool isLeft=false, bool isTop=false, bool isRight =false, bool isBottom=false )
        {
            if(isLeft)
                _excelWorksheet.Cells[adress].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            if (isTop)
                _excelWorksheet.Cells[adress].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            if (isRight)
                _excelWorksheet.Cells[adress].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            if (isBottom)
                _excelWorksheet.Cells[adress].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        }
    }
}
