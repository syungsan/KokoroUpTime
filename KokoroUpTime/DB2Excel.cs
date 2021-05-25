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

        private static DataChapter1 dataChapter1 = new DataChapter1();

        private static DataChapter2 dataChapter2 = new DataChapter2();

        private static DataChapter3 dataChapter3 = new DataChapter3();

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

                excel.WriteCell(playSceneNumCells[index], playSceneNum + 1);

                excel.WriteCell(playSceneNameCells[index], datProgs[index]);
            }

            //第1回に書き込み
            if(dataProgress.HasCompletedChapter1 == true)
            {
                excel.SetSheet("第1回");

                var chapter1StrResult = new Dictionary<string, string>
                {
                    { "A4", dataChapter1.MyKindOfGoodFeelings },
                    { "B5", dataChapter1.MyKindOfBadFeelings },
                    { "F4", dataChapter1.KimisKindOfFeeling },
                    { "F8", dataChapter1.AkamarusKindOfFeeling },
                    { "F12", dataChapter1.AosukesKindOfFeeling },
                };

                var chapter1IntResult = new Dictionary<string, int?>
                {
                    { "G8", dataChapter1.AkamarusSizeOfFeeling },
                    { "G12", dataChapter1.AosukesSizeOfFeeling },
                };

                foreach (KeyValuePair<string, string> item in chapter1StrResult)
                {
                    excel.WriteCell(item.Key, item.Value);
                }

                foreach (KeyValuePair<string, int?> item in chapter1IntResult)
                {
                    excel.WriteCell(item.Key, item.Value);
                }
            }


            // ###############################################################################

            //第２回に書き込み
            if (dataProgress.HasCompletedChapter2 == true)
            {
                excel.SetSheet("第2回");

                var chapter2StrResult = new Dictionary<string, string>
                {
                    {"A4" , dataChapter2.MySelectGoodEvents},
                    {"F5" , dataChapter2.AosukesDifficultyOfEating},
                    {"F6" , dataChapter2.AosukesDifficultyOfGettingHighScore},
                    {"F7" , dataChapter2.AosukesDifficultyOfTalkingWithFriend},
                    {"H4" , dataChapter2.MyALittlleExcitingEvents},
                };

                var chapter2IntResult = new Dictionary<string, int?>
                {
                    {"E5" , dataChapter2.AosukesSizeOfFeelingOfEating},
                    {"E6" , dataChapter2.AosukesSizeOfFeelingOfGettingHighScore},
                    {"E7" , dataChapter2.AosukesSizeOfFeelingOfTalkingWithFriend},
                };

                foreach (KeyValuePair<string, string> item in chapter2StrResult)
                {
                    excel.WriteCell(item.Key, item.Value);
                }

                foreach (KeyValuePair<string, int?> item in chapter2IntResult)
                {
                    excel.WriteCell(item.Key, item.Value);
                }

            }

            // ###############################################################################


            //第3回に書き込み
            if (dataProgress.HasCompletedChapter3 == true)
            {
                excel.SetSheet("第3回");

                var chapter3StrResult = new Dictionary<string, string>
                {
                    { "D3", dataChapter3.AosukesKindOfFeelingPreUseItem },
                    { "D5", dataChapter3.KimisKindOfFeelingPreUseItem },
                    { "D9", dataChapter3.AosukesKindOfFeelingAfterUsedItem },
                    { "D11", dataChapter3.KimisKindOfFeelingAfterUsedItem },
                    { "D13", dataChapter3.AkamarusKindOfFeelingAfterUsedItem },
                    { "G2", dataChapter3.SelectedPraiseHotWord },
                    { "G3", dataChapter3.SelectedWorryHotWord },
                    { "G4", dataChapter3.SelectedEncourageHotWord },
                    { "G5", dataChapter3.SelectedThanksHotWord },
                };

                var chapter3IntResult = new Dictionary<string, int?>
                {
                    { "D4", dataChapter3.AosukesSizeOfFeelingPreUseItem },
                    { "D6", dataChapter3.KimisSizeOfFeelingPreUseItem },
                    { "D10", dataChapter3.AosukesSizeOfFeelingAfterUsedItem },
                    { "D12", dataChapter3.KimisSizeOfFeelingAfterUsedItem },
                    { "D14", dataChapter3.AkamarusSizeOfFeelingAfterUsedItem },
                };

                foreach (KeyValuePair<string, string> item in chapter3StrResult)
                {
                    excel.WriteCell(item.Key, item.Value);
                }

                foreach (KeyValuePair<string, int?> item in chapter3IntResult)
                {
                    excel.WriteCell(item.Key, item.Value);
                }
            }

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

                    dataOption.Is3SecondRule = row.Is3SecondRule;
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

                var resultChapter1 = connection.Query<DataChapter1>($"SELECT * FROM 'DataChapter1';");

                foreach (var row in resultChapter1)
                {
                    if (row.MyKindOfGoodFeelings != null)
                        dataChapter1.MyKindOfGoodFeelings = row.MyKindOfGoodFeelings;

                    if (row.MyKindOfBadFeelings != null)
                        dataChapter1.MyKindOfBadFeelings = row.MyKindOfBadFeelings;

                    if (row.KimisKindOfFeeling != null)
                        dataChapter1.KimisKindOfFeeling = row.KimisKindOfFeeling;

                    if (row.AkamarusKindOfFeeling != null)
                        dataChapter1.AkamarusKindOfFeeling = row.AkamarusKindOfFeeling;

                    if (row.AkamarusSizeOfFeeling != null)
                        dataChapter1.AkamarusSizeOfFeeling = row.AkamarusSizeOfFeeling;

                    if (row.AosukesKindOfFeeling != null)
                        dataChapter1.AosukesKindOfFeeling = row.AosukesKindOfFeeling;

                    if (row.AosukesSizeOfFeeling != null)
                        dataChapter1.AosukesSizeOfFeeling = row.AosukesSizeOfFeeling;
                }

                /*
                var result2 = connection.Query<DataChapter2>($@"SELECT * FROM DataChapter2' WHERE CreatedAt = '{dataChapter2.CreatedAt}';");

                foreach (var row in result2)
                {
                    dataChapter2.MySelectGoodEvents = row.MySelectGoodEvents;

                    dataChapter2.AosukesSizeOfFeelingOfEating = row.AosukesSizeOfFeelingOfEating;

                    dataChapter2.AosukesDifficultyOfEating = row.AosukesDifficultyOfEating;

                    dataChapter2.AosukesSizeOfFeelingOfGettingHighScore = row.AosukesSizeOfFeelingOfGettingHighScore;

                    dataChapter2.AosukesDifficultyOfGettingHighScore = row.AosukesDifficultyOfGettingHighScore;

                    dataChapter2.AosukesSizeOfFeelingOfTalkingWithFriend = row.AosukesSizeOfFeelingOfTalkingWithFriend;

                    dataChapter2.AosukesDifficultyOfTalkingWithFriend = row.AosukesDifficultyOfTalkingWithFriend;

                    dataChapter2.MyALittlleExcitingEvents = row.MyALittlleExcitingEvents;
                }
                */

                var resultChapter3 = connection.Query<DataChapter3>("SELECT * FROM 'DataChapter3';");

                foreach (var row in resultChapter3)
                {
                    if (row.AosukesKindOfFeelingPreUseItem != null)
                        dataChapter3.AosukesKindOfFeelingPreUseItem = row.AosukesKindOfFeelingPreUseItem;

                    if (row.KimisKindOfFeelingPreUseItem != null)
                        dataChapter3.KimisKindOfFeelingPreUseItem = row.KimisKindOfFeelingPreUseItem;

                    if (row.AosukesKindOfFeelingAfterUsedItem != null)
                        dataChapter3.AosukesKindOfFeelingAfterUsedItem = row.AosukesKindOfFeelingAfterUsedItem;

                    if (row.KimisKindOfFeelingAfterUsedItem != null)
                        dataChapter3.KimisKindOfFeelingAfterUsedItem = row.KimisKindOfFeelingAfterUsedItem;

                    if (row.AkamarusKindOfFeelingAfterUsedItem != null)
                        dataChapter3.AkamarusKindOfFeelingAfterUsedItem = row.AkamarusKindOfFeelingAfterUsedItem;

                    if (row.AosukesSizeOfFeelingPreUseItem != null)
                        dataChapter3.AosukesSizeOfFeelingPreUseItem = row.AosukesSizeOfFeelingPreUseItem;

                    if (row.KimisSizeOfFeelingPreUseItem != null)
                        dataChapter3.KimisSizeOfFeelingPreUseItem = row.KimisSizeOfFeelingPreUseItem;

                    if (row.AosukesSizeOfFeelingAfterUsedItem != null)
                        dataChapter3.AosukesSizeOfFeelingAfterUsedItem = row.AosukesSizeOfFeelingAfterUsedItem;

                    if (row.KimisSizeOfFeelingAfterUsedItem != null)
                        dataChapter3.KimisSizeOfFeelingAfterUsedItem = row.KimisSizeOfFeelingAfterUsedItem;

                    if (row.AkamarusSizeOfFeelingAfterUsedItem != null)
                        dataChapter3.AkamarusSizeOfFeelingAfterUsedItem = row.AkamarusSizeOfFeelingAfterUsedItem;

                    if (row.SelectedPraiseHotWord != null)
                        dataChapter3.SelectedPraiseHotWord = row.SelectedPraiseHotWord;

                    if (row.SelectedWorryHotWord != null)
                        dataChapter3.SelectedWorryHotWord = row.SelectedWorryHotWord;

                    if (row.SelectedEncourageHotWord != null)
                        dataChapter3.SelectedEncourageHotWord = row.SelectedEncourageHotWord;

                    if (row.SelectedThanksHotWord != null)
                        dataChapter3.SelectedThanksHotWord = row.SelectedThanksHotWord;
                }
            }
        }
    }
}
