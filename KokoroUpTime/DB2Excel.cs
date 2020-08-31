using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using System.Windows;
using SQLite;
using CsvReadWrite;
using System.IO;
using ExcelManage;
using Expansion;
using System.Diagnostics;

namespace KokoroUpTime
{
    class DB2Excel
    {
        // 一応データモデルの使用
        // private static InitConfig initConfig = new InitConfig();
        private static DataOption dataOption = new DataOption();
        private static DataItem dataItem = new DataItem();
        private static DataProgress dataProgress = new DataProgress();

        public static void WriteDB2Excel(string dbPath, string outputPath, string[] userInfos)
        {
            GetFullInfoFromDB(dbPath);

            List<List<string>> allScenes = new List<List<string>>();

            // 各回のシナリオファイルから構成シーン数をリストとして取得
            for (int i = 1; i <= 12; i++)
            {
                var file = $"./Scenarios/chapter{i}.csv";

                List<string> scenes = new List<string>();

                // ファイルが存在しない場合は空のシーンリストを追加
                if (File.Exists(file))
                {
                    using (var csv = new CsvReader(file))
                    {
                        var scenarios = csv.ReadToEnd();

                        foreach (var scenario in scenarios)
                        {
                            if (scenario[0] == "scene")
                            {
                                scenes.Add(scenario[1]);
                            }
                        }
                    }
                }
                allScenes.Add(scenes);
            }

            var excel = new ExcelManager();
            excel.Open(outputPath);

            // まとめシートに書き込み #######################################################

            // ループで正確にセルに記録するために配列を回す
            string[] datProgs = { dataProgress.LatestChapter1Scene, dataProgress.LatestChapter2Scene, dataProgress.LatestChapter3Scene, dataProgress.LatestChapter4Scene, dataProgress.LatestChapter5Scene, dataProgress.LatestChapter6Scene, dataProgress.LatestChapter7Scene, dataProgress.LatestChapter8Scene, dataProgress.LatestChapter9Scene, dataProgress.LatestChapter10Scene, dataProgress.LatestChapter11Scene, dataProgress.LatestChapter12Scene };
            string[] sceneNumCells = { "B9", "B10", "B11", "B12", "B13", "B14", "B15", "B16", "B17", "B18", "B19", "B20" };
            string[] playSceneNumCells = { "C9", "C10", "C11", "C12", "C13", "C14", "C15", "C16", "C17", "C18", "C19", "C20" };
            string[] playSceneNameCells = { "D9", "D10", "D11", "D12", "D13", "D14", "D15", "D16", "D17", "D18", "D19", "D20" };

            excel.SetSheet("まとめ");

            // プレイヤー名
            excel.WriteCell("B2", $"{userInfos[0]}{userInfos[1]}");

            // アクセス日
            excel.WriteCell("B5", $"{userInfos[2]}");

            foreach (var (allScene, index) in allScenes.Indexed())
            {
                excel.WriteCell(sceneNumCells[index], allScene.Count);

                var playSceneNum = allScene.IndexOf(datProgs[index]);

                excel.WriteCell(playSceneNumCells[index], playSceneNum);

                excel.WriteCell(playSceneNameCells[index], datProgs[index]);
            }

            // ###############################################################################

            // 名前を付けて保存する
            if (excel.SaveAs(outputPath) == false)
            {
                MessageBox.Show("ファイルが既に開かれています。\n閉じてから、再試行してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 各ユーザのデータベースの内容を全て吸い上げるための関数
        private static void GetFullInfoFromDB(string dbPath)
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                var option = connection.Query<DataOption>("SELECT * FROM DataOption WHERE Id = 1;");

                foreach (var row in option)
                {
                    dataOption.InputMethod = row.InputMethod;

                    dataOption.IsPlaySE = row.IsPlaySE;

                    dataOption.IsPlayBGM = row.IsPlayBGM;

                    dataOption.MessageSpeed = row.MessageSpeed;

                    dataOption.IsAddRubi = row.IsAddRubi;
                }

                var item = connection.Query<DataItem>("SELECT * FROM DataItem WHERE Id = 1;");

                foreach (var row in item)
                {
                    dataItem.HasGotItem01 = row.HasGotItem01;

                    dataItem.HasGotItem02 = row.HasGotItem02;

                    dataItem.HasGotItem03 = row.HasGotItem03;

                    dataItem.HasGotItem04 = row.HasGotItem04;

                    dataItem.HasGotItem05 = row.HasGotItem05;

                    dataItem.HasGotItem06 = row.HasGotItem06;

                    dataItem.HasGotItem07 = row.HasGotItem07;

                    dataItem.HasGotItem08 = row.HasGotItem08;

                    dataItem.HasGotItem09 = row.HasGotItem09;

                    dataItem.HasGotItem10 = row.HasGotItem10;

                    dataItem.HasGotItem11 = row.HasGotItem11;
                }

                var progress = connection.Query<DataProgress>("SELECT * FROM DataProgress WHERE Id = 1;");

                foreach (var row in progress)
                {
                    dataProgress.CurrentChapter = row.CurrentChapter;

                    dataProgress.CurrentScene = row.CurrentScene;

                    dataProgress.LatestChapter1Scene = row.LatestChapter1Scene;

                    dataProgress.HasCompletedChapter1 = row.HasCompletedChapter1;

                    dataProgress.LatestChapter2Scene = row.LatestChapter2Scene;

                    dataProgress.HasCompletedChapter2 = row.HasCompletedChapter2;

                    dataProgress.LatestChapter3Scene = row.LatestChapter3Scene;

                    dataProgress.HasCompletedChapter3 = row.HasCompletedChapter3;

                    dataProgress.LatestChapter4Scene = row.LatestChapter4Scene;

                    dataProgress.HasCompletedChapter4 = row.HasCompletedChapter4;

                    dataProgress.LatestChapter5Scene = row.LatestChapter5Scene;

                    dataProgress.HasCompletedChapter5 = row.HasCompletedChapter5;

                    dataProgress.LatestChapter6Scene = row.LatestChapter6Scene;

                    dataProgress.HasCompletedChapter6 = row.HasCompletedChapter6;

                    dataProgress.LatestChapter7Scene = row.LatestChapter7Scene;

                    dataProgress.HasCompletedChapter7 = row.HasCompletedChapter7;

                    dataProgress.LatestChapter8Scene = row.LatestChapter8Scene;

                    dataProgress.HasCompletedChapter8 = row.HasCompletedChapter8;

                    dataProgress.LatestChapter9Scene = row.LatestChapter9Scene;

                    dataProgress.HasCompletedChapter9 = row.HasCompletedChapter9;

                    dataProgress.LatestChapter10Scene = row.LatestChapter10Scene;

                    dataProgress.HasCompletedChapter10 = row.HasCompletedChapter10;

                    dataProgress.LatestChapter11Scene = row.LatestChapter11Scene;

                    dataProgress.HasCompletedChapter11 = row.HasCompletedChapter11;

                    dataProgress.LatestChapter12Scene = row.LatestChapter12Scene;

                    dataProgress.HasCompletedChapter12 = row.HasCompletedChapter12;
                }
            }
        }
    }
}
