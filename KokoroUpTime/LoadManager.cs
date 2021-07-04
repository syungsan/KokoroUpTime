using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Controls;

namespace KokoroUpTime
{
    public class LoadManager
    {
        public void LoadDataChapterFromDB<TDataChapter>(TDataChapter _dataChapter, string dbPath)
        {
            //DataChapterの型を取得
            var typeOfDataChapter = _dataChapter.GetType();

            //取得したDataChapterの全Propertyを取得
            PropertyInfo[] dataChapterPropertyInfos = typeOfDataChapter.GetProperties();

            using (var connection = new SQLiteConnection(dbPath))
            {
                string chapterNumber = typeOfDataChapter.Name.Replace("DataChapter", "");
                switch (chapterNumber)
                {
                    case "1":
                        var resultChapter1 = connection.Query<DataChapter1>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        foreach (var dataChapterProp in dataChapterPropertyInfos)
                        {
                            var dbProp = typeof(DataChapter1).GetProperty(dataChapterProp.Name);
                            dataChapterProp.SetValue(_dataChapter, dbProp.GetValue(resultChapter1));
                        }
                        break;
                    case "2":
                        var resultChapter2 = connection.Query<DataChapter2>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        foreach (var dataChapterProp in dataChapterPropertyInfos)
                        {
                            var dbProp = typeof(DataChapter2).GetProperty(dataChapterProp.Name);
                            dataChapterProp.SetValue(_dataChapter, dbProp.GetValue(resultChapter2));
                        }
                        break;
                    case "3":
                        var resultChapter3 = connection.Query<DataChapter3>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        foreach (var dataChapterProp in dataChapterPropertyInfos)
                        {
                            var dbProp = typeof(DataChapter3).GetProperty(dataChapterProp.Name);
                            dataChapterProp.SetValue(_dataChapter, dbProp.GetValue(resultChapter3));
                        }
                        break;
                    case "4":
                        var resultChapter4 = connection.Query<DataChapter4>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        foreach (var dataChapterProp in dataChapterPropertyInfos)
                        {
                            var dbProp = typeof(DataChapter4).GetProperty(dataChapterProp.Name);
                            dataChapterProp.SetValue(_dataChapter, dbProp.GetValue(resultChapter4));
                        }
                        break;
                    case "5":
                        var resultChapter5 = connection.Query<DataChapter5>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        foreach (var dataChapterProp in dataChapterPropertyInfos)
                        {
                            var dbProp = typeof(DataChapter5).GetProperty(dataChapterProp.Name);
                            dataChapterProp.SetValue(_dataChapter, dbProp.GetValue(resultChapter5));
                        }

                        break;
                    case "6":
                        var resultChapter6 = connection.Query<DataChapter6>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        foreach (var dataChapterProp in dataChapterPropertyInfos)
                        {
                            var dbProp = typeof(DataChapter6).GetProperty(dataChapterProp.Name);
                            dataChapterProp.SetValue(_dataChapter, ((string)dbProp.GetValue(resultChapter6)).Split(";").ToList());
                        }
                        break;
                    case "7":
                        var resultChapter7 = connection.Query<DataChapter7>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        foreach (var dataChapterProp in dataChapterPropertyInfos)
                        {
                            var dbProp = typeof(DataChapter7).GetProperty(dataChapterProp.Name);
                            dataChapterProp.SetValue(_dataChapter, ((string)dbProp.GetValue(resultChapter7)).Split(";").ToList());
                        }
                        break;
                    case "8":
                        var resultChapter8 = connection.Query<DataChapter8>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        foreach (var dataChapterProp in dataChapterPropertyInfos)
                        {
                            var dbProp = typeof(DataChapter8).GetProperty(dataChapterProp.Name);
                            dataChapterProp.SetValue(_dataChapter, ((string)dbProp.GetValue(resultChapter8)).Split(";").ToList());
                        }
                        break;
                    case "9":
                        var resultChapter9 = connection.Query<DataChapter9>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        foreach (var dataChapterProp in dataChapterPropertyInfos)
                        {
                            var dbProp = typeof(DataChapter9).GetProperty(dataChapterProp.Name);
                            dataChapterProp.SetValue(_dataChapter, ((string)dbProp.GetValue(resultChapter9)).Split(";").ToList());
                        }
                        break;
                    case "10":
                        var resultChapter10 = connection.Query<DataChapter10>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        foreach (var dataChapterProp in dataChapterPropertyInfos)
                        {
                            var dbProp = typeof(DataChapter10).GetProperty(dataChapterProp.Name);
                            dataChapterProp.SetValue(_dataChapter, ((string)dbProp.GetValue(resultChapter10)).Split(";").ToList());
                        }
                        break;
                    case "11":
                        var resultChapter11 = connection.Query<DataChapter11>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        foreach (var dataChapterProp in dataChapterPropertyInfos)
                        {
                            var dbProp = typeof(DataChapter11).GetProperty(dataChapterProp.Name);
                            dataChapterProp.SetValue(_dataChapter, ((string)dbProp.GetValue(resultChapter11)).Split(";").ToList());
                        }
                        break;
                    case "12":
                        var resultChapter12 = connection.Query<DataChapter12>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        foreach (var dataChapterProp in dataChapterPropertyInfos)
                        {
                            var dbProp = typeof(DataChapter12).GetProperty(dataChapterProp.Name);
                            dataChapterProp.SetValue(_dataChapter, ((string)dbProp.GetValue(resultChapter12)).Split(";").ToList());
                        }
                        break;
                }
            }
        }
        public void ToListBox(ListBox listBox,List<string> items)
        {
            foreach (var item in items)
            {
                if (listBox.Items.Contains(item))
                {
                    try{
                        var listBoxItem = listBox.Items.GetItemAt(listBox.Items.IndexOf(item)) as ListBoxItem;
                        listBoxItem.IsSelected = true;
                    }
                    catch
                    {
                        Debug.Print($"ロードに失敗しました オブジェクト名：{listBox.Name}");
                    }
                    
                }
            }
        }
        public void ToListBox(ListBox listBox, string item)
        {
            if (listBox.Items.Contains(item))
            {
                try
                {
                    var listBoxItem = listBox.Items.GetItemAt(listBox.Items.IndexOf(item)) as ListBoxItem;
                    listBoxItem.IsSelected = true;
                }
                catch
                {
                    Debug.Print($"ロードに失敗しました オブジェクト名：{listBox.Name}");
                }
            }
        }
        public void ToIncCanvas(InkCanvas canvas, string path)
        {
            if (File.Exists(path))
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    canvas.Strokes = new System.Windows.Ink.StrokeCollection(fs);
                }
            }
            
        }
        public void ToTextBlock(TextBlock textBlock, string data)
        {
            textBlock.Text = data;
        }
        public void ToListView(ListView listView, string data)
        {

        }
        public void ToItemsControl(ItemsControl itemsControl, string data)
        {

        }
        public void To()
        {

        }
    }
}
