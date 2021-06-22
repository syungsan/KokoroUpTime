using CsvReadWrite;
using ExcelManage;
using Expansion;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using FileIOUtils;
using OfficeOpenXml;

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
        private static DataChapter4 dataChapter4 = new DataChapter4();
        private static DataChapter5 dataChapter5 = new DataChapter5();
        private static DataChapter6 dataChapter6 = new DataChapter6();
        private static DataChapter7 dataChapter7 = new DataChapter7();
        private static DataChapter8 dataChapter8 = new DataChapter8();
        private static DataChapter9 dataChapter9 = new DataChapter9();
        private static DataChapter10 dataChapter10 = new DataChapter10();
        private static DataChapter11 dataChapter11 = new DataChapter11();
        private static DataChapter12 dataChapter12 = new DataChapter12();

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


            string startupPath = FileUtils.GetStartupPath();
            string dirPath = $"./Log/{userInfos[0]}";

            //第1回に書き込み
            if (dataProgress.HasCompletedChapter1 == true)
            {
                excel.SetSheet("第1回");

                var chapter1StrResult = new Dictionary<string, string>
                {
                    { "F4", dataChapter1.KimisKindOfFeeling },
                    { "F8", dataChapter1.AkamarusKindOfFeeling },
                    { "F12", dataChapter1.AosukesKindOfFeeling },
                };


                if (dataChapter1.MyKindOfGoodFeelings !="")
                {
                    IEnumerable<string> goodFeelings = dataChapter1.MyKindOfGoodFeelings.Split(",");
                    foreach (var goodfeel in goodFeelings.Select((Value, Index) => new { Value, Index }))
                    {
                        excel.InsertCells("A4", ExcelManager.InsertMode.ShiftDown);
                        excel.DrawBorder($"A{4 + goodfeel.Index}", true, true, true, true);
                        chapter1StrResult.Add($"A{4 + goodfeel.Index}", goodfeel.Value);
                    }
                }

                if (dataChapter1.MyKindOfBadFeelings != "")
                {
                    IEnumerable<string> badFeelings = dataChapter1.MyKindOfBadFeelings.Split(",");
                    foreach (var badfeel in badFeelings.Select((Value, Index) => new { Value, Index }))
                    {
                        excel.InsertCells("B4",ExcelManager.InsertMode.ShiftDown);
                        excel.DrawBorder($"B{4 + badfeel.Index}", true, true, true, true);
                        chapter1StrResult.Add($"B{4 + badfeel.Index}", badfeel.Value);
                    }
                }

                var chapter1IntResult = new Dictionary<string, int?>
                {
                    { "G8", dataChapter1.AkamarusSizeOfFeeling },
                    { "G12", dataChapter1.AosukesSizeOfFeeling },
                };

                foreach (KeyValuePair<string, string> item in chapter1StrResult)
                {
                    if (item.Value.Contains(",良い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",良い", ""));
                    }
                    else if (item.Value.Contains(",悪い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",悪い", ""));
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);

                    }

                    //if (item.Value.Contains("●"))
                    //{
                    //    excel.WriteCell(item.Key, item.Value.Replace("●　", ""));
                    //}
                }

                foreach (KeyValuePair<string, int?> item in chapter1IntResult)
                {
                    if (item.Value ==-1)
                    {
                        excel.WriteCell(item.Key, null);
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);
                    }
                }
                excel.AutoFitColumns();
            }

            //第2回に書き込み
            if (dataProgress.HasCompletedChapter2 == true)
            {
                excel.SetSheet("第2回");

                var chapter2StrResult = new Dictionary<string, string>
                {
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

                if (dataChapter2.MySelectGoodEvents !="")
                {
                    IEnumerable<string> goodEvents = dataChapter2.MySelectGoodEvents.Split(",");
                    foreach (var goodEvent in goodEvents.Select((Value, Index) => new { Value, Index }))
                    {
                        
                        excel.InsertCells("A4",ExcelManager.InsertMode.ShiftDown);
                        excel.DrawBorder($"A{4 + goodEvent.Index}",true,true,true,true);
                        chapter2StrResult.Add($"A{4 + goodEvent.Index}", goodEvent.Value);
                    }
                }

                if (dataChapter2.MyALittlleExcitingEvents =="")
                {
                    excel.WriteCell($"C10", "");

                    string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter2/groupe_activity_exciting_event_stroke.isf");
                    if (File.Exists(logDataPath))
                    {
                      excel.PastePicture($"C10","groupe_activity_exciting_event_stroke.png", logDataPath, 40);
                    }
                }
                else
                {
                    chapter2StrResult.Add($"C10", dataChapter2.MyALittlleExcitingEvents);
                }



                foreach (KeyValuePair<string, string> item in chapter2StrResult)
                {
                    if (item.Value.Contains(",良い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",良い", ""));
                    }
                    else if (item.Value.Contains(",悪い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",悪い", ""));
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);

                    }

                    //if (item.Value.Contains("●"))
                    //{
                    //    excel.WriteCell(item.Key, item.Value.Replace("●　", ""));
                    //}
                }

                foreach (KeyValuePair<string, int?> item in chapter2IntResult)
                {
                    if (item.Value == -1)
                    {
                        excel.WriteCell(item.Key, null);
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);
                    }
                }
                excel.AutoFitColumns();
                
            }

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
                    if (item.Value.Contains(",良い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",良い", ""));
                    }
                    else if (item.Value.Contains(",悪い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",悪い", ""));
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);

                    }
                }

                foreach (KeyValuePair<string, int?> item in chapter3IntResult)
                {
                    if (item.Value == -1)
                    {
                        excel.WriteCell(item.Key, null);
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);
                    }
                }
                excel.AutoFitColumns();
            }

            //第4回に書き込み
            if (dataProgress.HasCompletedChapter4 == true)
            {
                excel.SetSheet("第4回");

                var chapter4StrResult = new Dictionary<string, string>
                {
                    { "B4", dataChapter4.KimisKindOfFeelingAskedForWork },
                    { "B5", dataChapter4.KimisKindOfFeelingAskedByAkamaru },
                };

                var chapter4IntResult = new Dictionary<string, int?>
                {
                   { "C4", dataChapter4.KimisSizeOfFeelingAskedForWork },
                   { "C5", dataChapter4.KimisSizeOfFeelingAskedByAkamaru },
                };

                foreach (KeyValuePair<string, string> item in chapter4StrResult)
                {
                    if (item.Value.Contains(",良い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",良い", ""));
                    }
                    else if (item.Value.Contains(",悪い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",悪い", ""));
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);

                    }

                    //if (item.Value.Contains("●"))
                    //{
                    //    excel.WriteCell(item.Key, item.Value.Replace("●　", ""));
                    //}
                }

                foreach (KeyValuePair<string, int?> item in chapter4IntResult)
                {
                    if (item.Value == -1)
                    {
                        excel.WriteCell(item.Key, null);
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);
                    }
                }
                excel.AutoFitColumns();

            }

            //第5回に書き込み
            if (dataProgress.HasCompletedChapter5 == true)
            {
                excel.SetSheet("第5回");

                var chapter5StrResult = new Dictionary<string, string>
                {
                    { "E4", dataChapter5.KindOfFeelingNotUnderstandProblem },
                    { "E5", dataChapter5.KindOfFeelingRecorderProblem },

                };

                var chapter5IntResult = new Dictionary<string, int?>
                {
                    { "F4", dataChapter5.SizeOfFeelingNotUnderstandProblem },
                    { "F5", dataChapter5.SizeOfFeelingRecorderProblem },
                };


                //todo:全部の体の部位のテキストが空の時
                if (dataChapter5.InputFaceImageOfKimiText =="")
                {
                    excel.WriteCell($"B4", "");
                    string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter5/challenge_time_face_image_of_kimi.isf");
                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"B4", $"challenge_time_face_image_of_kimi.png", logDataPath, 40);
                    }

                    excel.WriteCell($"B5", "");
                    logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter5/challenge_time_head_image_of_kimi.isf");
                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"B5", $"challenge_time_head_image_of_kimi.png", logDataPath, 40);
                    }

                    excel.WriteCell($"B6", "");
                    logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter5/challenge_time_hand_image_of_kimi.isf");
                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"B6", $"challenge_time_hand_image_of_kimi.png", logDataPath, 40);
                    }

                    excel.WriteCell($"B7", "");
                    logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter5/challenge_time_shoulder_image_of_kimi.isf");
                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"B7", $"challenge_time_shoulder_image_of_kimi.png", logDataPath, 40);
                　　}

                    excel.WriteCell($"B8", "");
                    logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter5/challenge_time_stomach_image_of_kimi.isf");
                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"B8", $"challenge_time_stomach_image_of_kimi.png", logDataPath, 40);
                    }

                    excel.WriteCell($"B9", "");
                    logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter5/challenge_time_leg_image_of_kimi.isf");
                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"B9", $"challenge_time_leg_image_of_kimi.png", logDataPath, 40);
                    }

                    excel.WriteCell($"B10", "");
                    logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter5/challenge_time_others_image_of_kimi.isf");
                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"B10", $"challenge_time_others_image_of_kimi.png", logDataPath, 40);
                    }
                }
                else
                {
                    chapter5StrResult.Add("B4", dataChapter5.InputFaceImageOfKimiText);
                    chapter5StrResult.Add("B5", dataChapter5.InputHeadImageOfKimiText);
                    chapter5StrResult.Add("B6", dataChapter5.InputHandImageOfKimiText);
                    chapter5StrResult.Add("B7", dataChapter5.InputShoulderImageOfKimiText);
                    chapter5StrResult.Add("B8", dataChapter5.InputStomachImageOfKimiText);
                    chapter5StrResult.Add("B9", dataChapter5.InputLegImageOfKimiText);
                    chapter5StrResult.Add("B10", dataChapter5.InputOthersImageOfKimiText);
                }

                if (dataChapter5.InputMyBodyImageTextNotUnderstandProblem =="" && dataChapter5.InputMyBodyImageTextRecorderProblem =="")
                {
                    string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter5/challenge_time_not_understand_problem_input_stroke.isf");
                    excel.WriteCell($"G4", "");

                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"G4", $"challenge_time_not_understand_problem_input_stroke.png", logDataPath, 17);
                    }

                    logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter5/challenge_time_recorder_problem_input_stroke.isf");
                    excel.WriteCell($"G5", "");

                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"G5", $"challenge_time_recorder_problem_input_stroke.png", logDataPath, 17);
                    }
                }
                else
                {
                    chapter5StrResult.Add("G4", dataChapter5.InputMyBodyImageTextNotUnderstandProblem);
                    chapter5StrResult.Add("G5", dataChapter5.InputMyBodyImageTextRecorderProblem );
                }

                if (dataChapter5.InputRelaxMethodText =="")
                {
                    for (int i=0;i<5;i++)
                    {
                        string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter5/groupe_avtivity_relax_method_{i+1}.isf");
                        excel.WriteCell($"C10", "");

                        if (File.Exists(logDataPath))
                        {
                            chapter5IntResult.Add($"I{4 + i}", i + 1);
                            excel.PastePicture($"j{4+i}", $"groupe_avtivity_relax_method_{i+1}.png", logDataPath, 40);
                        }
                    }
                }
                else
                {
                    IEnumerable<string> relaxMethods = dataChapter5.InputRelaxMethodText.Split(",");
                    foreach (var relaxMethod in relaxMethods.Select((Value, Index) => new { Value, Index }))
                    {
                       

                        excel.InsertCells("I4", ExcelManager.InsertMode.ShiftDown);
                        excel.InsertCells("J4",ExcelManager.InsertMode.ShiftDown);

                        excel.DrawBorder($"I{4 + relaxMethod.Index}", true, true, true, true);
                        excel.DrawBorder($"J{4 + relaxMethod.Index}", true, true, true, true);

                        chapter5IntResult.Add($"I{4 + relaxMethod.Index}", relaxMethod.Index + 1);
                        chapter5StrResult.Add($"J{4 + relaxMethod.Index}", relaxMethod.Value);
                    }
                }


                foreach (KeyValuePair<string, string> item in chapter5StrResult)
                {
                    if (item.Value.Contains(",良い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",良い", ""));
                    }
                    else if (item.Value.Contains(",悪い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",悪い", ""));
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);

                    }

                    //if (item.Value.Contains("●"))
                    //{
                    //    excel.WriteCell(item.Key, item.Value.Replace("●　", ""));
                    //}
                }

                foreach (KeyValuePair<string, int?> item in chapter5IntResult)
                {
                    if (item.Value == -1)
                    {
                        excel.WriteCell(item.Key, null);
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);
                    }
                }
                excel.AutoFitColumns();
            }

            //第6回に書き込み
            if (dataProgress.HasCompletedChapter6 == true)
            {
                excel.SetSheet("第6回");

                var chapter6StrResult = new Dictionary<string, string>();


                if (dataChapter6.SelectedNicePersonality !="")
                {
                    IEnumerable<string> nicePersonalities = dataChapter6.SelectedNicePersonality.Split(",");
                    foreach (var nicePersonality in nicePersonalities.Select((Value, Index) => new { Value, Index }))
                    {
                        excel.InsertCells("A4", ExcelManager.InsertMode.ShiftDown);
                        excel.DrawBorder($"A{3 + nicePersonality.Index}", true, true, true, true);
                        chapter6StrResult.Add($"A{3 + nicePersonality.Index}", nicePersonality.Value);
                    }
                }


                if (dataChapter6.InputFriendsNicePersonality == "")
                {
                    for (int i=0;i<5;i++)
                    {
                        excel.WriteCell($"C{4+i}", "");
                        excel.WriteCell($"D{4+i}", "");

                        string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter6/groupe_activity_friends_name_{i+1}.isf");
                        if (File.Exists(logDataPath))
                        {
                            excel.InsertCells("A4", ExcelManager.InsertMode.ShiftDown);
                            excel.DrawBorder($"A{4 + i}", true, true, true, true);
                            excel.PastePicture($"C{4 + i}", $"groupe_activity_friends_name_{ i + 1}.png",logDataPath,40);
                        }

                        logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter6/groupe_activity_friends_personality_{i + 1}.isf");
                        if (File.Exists(logDataPath))
                        {
                            excel.InsertCells("B4", ExcelManager.InsertMode.ShiftDown);
                            excel.DrawBorder($"B{4 + i}", true, true, true, true);
                            excel.PastePicture($"D{4 + i}", $"groupe_activity_friends_personality_{ i + 1}.png", logDataPath, 40);
                        }
                    }
                }
                else
                {
                    IEnumerable<string> friendsNicePersonalities = dataChapter6.InputFriendsNicePersonality.Split(";");
                    foreach (var friendsNicePersonality in friendsNicePersonalities.Select((Value, Index) => new { Value, Index }))
                    {
                        if (friendsNicePersonality.Value !="")
                        {
                            chapter6StrResult.Add($"C{4 + friendsNicePersonality.Index}", friendsNicePersonality.Value.Split(",")[0]);
                            chapter6StrResult.Add($"D{4 + friendsNicePersonality.Index}", friendsNicePersonality.Value.Split(",")[1]);
                        }
                           
                    }
                }

                foreach (KeyValuePair<string, string> item in chapter6StrResult)
                {
                    if (item.Value.Contains(",良い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",良い", ""));
                    }
                    else if (item.Value.Contains(",悪い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",悪い", ""));
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);

                    }
                }
                excel.AutoFitColumns();

            }

            //第7回に書き込み
            if (dataProgress.HasCompletedChapter7 == true)
            {
                excel.SetSheet("第7回");

                var chapter7StrResult = new Dictionary<string, string>
                {
                    { "C4", dataChapter7.KimisKindOfFeelingAnnouncement },
                    { "C5", dataChapter7.KimisKindOfFeelingInviteFriends },
                   
                    { "A10", dataChapter7.ChallengeTimeSelectedScene},
                    { "F10", dataChapter7.GroupeActivitySelectedScene},
                };

                var chapter7IntResult = new Dictionary<string, int?>
                {
                   { "D4", dataChapter7.KimisSizeOfFeelingAnnouncement },
                   { "D5", dataChapter7.KimisSizeOfFeelingInviteFriends },
                };



                if (dataChapter7.InputAkamaruThoughtText==""&& dataChapter7.InputAosukeThoughtText=="")
                {
                    excel.WriteCell($"G4", "");
                    excel.WriteCell($"G6", "");

                    string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter7/groupe_activity_akamaru's_thought.isf");
                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"G4", $"groupe_activity_akamaru's_thought.png", logDataPath, 20);
                    }

                    logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter7/groupe_activity_aosuke's_thought.isf");
                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"G6", $"groupe_activity_aosuke's_thought.png", logDataPath, 20);
                    }

                }
                else
                {
                    chapter7StrResult.Add("G4", dataChapter7.InputAkamaruThoughtText);
                    chapter7StrResult.Add("G6", dataChapter7.InputAosukeThoughtText);
                }


                if (dataChapter7.ChallengeTimeSelectedScene == "音楽の発表で、みんなの前で失敗してしまいました。友だちの１人が、こっちを見て笑っていました。")
                {
                    if(dataChapter7.InputYourToughtText1 == "")//this.dataOption.InputMethod == 0))
                    {
                        excel.WriteCell($"B10", "");

                        string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter7/challenge_time_2_your_thought.isf");
                        if (File.Exists(logDataPath))
                        {
                            excel.PastePicture($"B10", $"challenge_time_2_your_thought.png", logDataPath, 20);
                        }
                    }
                    else
                    {
                        chapter7StrResult.Add("B10", dataChapter7.InputYourToughtText1);
                    }

                    chapter7StrResult.Add("C10", dataChapter7.YourKindOfFeelingAnnouncement);
                    chapter7IntResult.Add("D10", dataChapter7.YourSizeOfFeelingAnnouncement);

                }
                else
                {
                    if (dataChapter7.InputYourToughtText2== "")
                    {
                        excel.WriteCell($"B10", "");

                        string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter7/challenge_time_2_your_thought.isf");
                        if (File.Exists(logDataPath))
                        {
                            excel.PastePicture($"B10", $"challenge_time_2_your_thought.png", logDataPath, 20);
                        }
                    }
                    else
                    {
                        chapter7StrResult.Add("B10", dataChapter7.InputYourToughtText2);
                    }

                    chapter7StrResult.Add("C10", dataChapter7.YourKindOfFeelingGreetingToFriend);
                    chapter7IntResult.Add("D10", dataChapter7.YourSizeOfFeelingGreetingToFriend);
                }

                if (dataChapter7.GroupeActivitySelectedScene == "音楽の発表で、みんなの前で失敗してしまいました。友だちの１人が、こっちを見て笑っていました。")
                {
                    if (dataChapter7.InputFriendToughtText1 == "")//this.dataOption.InputMethod == 0))
                    {
                        excel.WriteCell($"G10", "");

                        string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter7/groupe_activity_2_your_friend's_thought.isf");
                        if (File.Exists(logDataPath))
                        {
                            excel.PastePicture($"G10", $"groupe_activity_2_your_friend's_thought.png", logDataPath, 20);
                        }
                    }
                    else
                    {
                        chapter7StrResult.Add("G10", dataChapter7.InputFriendToughtText1);
                    }

                    chapter7StrResult.Add("H10", dataChapter7.YourFriendsKindOfFeelingAnnouncement);
                    chapter7IntResult.Add("I10", dataChapter7.YourFriendsSizeOfFeelingAnnouncement);

                }
                else
                {
                    if (dataChapter7.InputFriendToughtText2 == "")
                    {
                        excel.WriteCell($"G10", "");

                        string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter7/groupe_activity_2_your_friend's_thought.isf");
                        if (File.Exists(logDataPath))
                        {
                            excel.PastePicture($"G10", $"groupe_activity_2_your_friend's_thought.png", logDataPath, 20);
                        }
                    }
                    else
                    {
                        chapter7StrResult.Add("G10", dataChapter7.InputFriendToughtText2);
                    }

                    chapter7StrResult.Add("H10", dataChapter7.YourFriendsKindOfFeelingGreetingToAnotherFriend);
                    chapter7IntResult.Add("I10", dataChapter7.YourFriendsSizeOfFeelingGreetingToAnotherFriend);
                }

                foreach (KeyValuePair<string, string> item in chapter7StrResult)
                {
                    if (item.Value.Contains(",良い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",良い", ""));
                    }
                    else if (item.Value.Contains(",悪い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",悪い", ""));
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);

                    }

                    //if (item.Value.Contains("●"))
                    //{
                    //    excel.WriteCell(item.Key, item.Value.Replace("●　", ""));
                    //}
                }

                foreach (KeyValuePair<string, int?> item in chapter7IntResult)
                {
                    if (item.Value == -1)
                    {
                        excel.WriteCell(item.Key, null);
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);
                    }
                }
                excel.AutoFitColumns();
            }

            //第8回に書き込み
            if (dataProgress.HasCompletedChapter8 == true)
            {
                excel.SetSheet("第8回");

                var chapter8StrResult = new Dictionary<string, string>
                {
                    { "C4", dataChapter8.KimisKindOfFeelingPreUseItem },
                    { "C5", dataChapter8.AosukesKindOfFeelingPreUseItem },
                    { "C6", dataChapter8.AkamarusKindOfFeelingPreUseItem },
                    { "E4", dataChapter8.KimisKindOfFeelingAfterUsedItem },
                    { "E5", dataChapter8.AosukesKindOfFeelingAfterUsedItem },
                    { "E6", dataChapter8.AkamarusKindOfFeelingAfterUsedItem },
                    { "J4", dataChapter8.Let_sCheckKindOfFeeling },
                    { "J5", dataChapter8.PositiveThinkingKindOfFeeling },
                    { "J6", dataChapter8.ThoughtsOfOthersKindOfFeeling },
                };

                var chapter8IntResult = new Dictionary<string, int?>
                {
                    { "D4", dataChapter8.KimisSizeOfFeelingPreUseItem },
                    { "D5", dataChapter8.AosukesSizeOfFeelingPreUseItem },
                    { "D6", dataChapter8.AkamarusSizeOfFeelingPreUseItem },
                    { "F4", dataChapter8.KimisSizeOfFeelingAfterUsedItem },
                    { "F5", dataChapter8.AosukesSizeOfFeelingAfterUsedItem },
                    { "F6", dataChapter8.AkamarusSizeOfFeelingAfterUsedItem },
                    { "K4", dataChapter8.Let_sCheckSizeOfFeeling },
                    { "K5", dataChapter8.PositiveThinkingSizeOfFeeling },
                    { "K6", dataChapter8.ThoughtsOfOthersSizeOfFeeling },
                };


                for (int i = 0;i<3;i++)
                {
                    var PlopInfo = typeof(DataChapter8).GetProperty($"GroupeActivityInputText{i + 1}");
                    if ((string)PlopInfo.GetValue(dataChapter8) == "")
                    {
                        excel.WriteCell($"I{4 + i}", "");

                        string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter8/groupe_activity_input_stroke_1{i + 1}.isf");
                        if (File.Exists(logDataPath))
                        {
                            excel.PastePicture($"I{4 + i}", $"groupe_activity_input_stroke_1{ i + 1}.png", logDataPath, 25);
                        }
                    }
                    else
                    {
                        chapter8StrResult.Add($"I{4 + i}", (string)PlopInfo.GetValue(dataChapter8));
                    }
                }



                foreach (KeyValuePair<string, string> item in chapter8StrResult)
                {
                    if (item.Value.Contains(",良い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",良い", ""));
                    }
                    else if (item.Value.Contains(",悪い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",悪い", ""));
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);

                    }

                    //if (item.Value.Contains("●"))
                    //{
                    //    excel.WriteCell(item.Key, item.Value.Replace("●　", ""));
                    //}
                }

                foreach (KeyValuePair<string, int?> item in chapter8IntResult)
                {
                    if (item.Value == -1)
                    {
                        excel.WriteCell(item.Key, null);
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);
                    }
                }

                excel.AutoFitColumns();
            }

            //第9回に書き込み
            if (dataProgress.HasCompletedChapter9 == true)
            {
                excel.SetSheet("第9回");

                var chapter9StrResult = new Dictionary<string, string>
                {
                    { "M4", dataChapter9.AosukesKindOfFeelingTalkToFriends },
                    { "M5", dataChapter9.AosukesKindOfFeelingAfter10minutes },
                    { "M6", dataChapter9.AosukesKindOfFeelingNextDay },
                    { "M7", dataChapter9.AosukesKindOfFeelingDayAfterTomorrow },
                };

                var chapter9IntResult = new Dictionary<string, int?>
                {
                    { "N4", dataChapter9.AosukesSizeOfFeelingTalkToFriends },
                    { "N5", dataChapter9.AosukesSizeOfFeelingAfter10minutes },
                    { "N6", dataChapter9.AosukesSizeOfFeelingNextDay },
                    { "N7", dataChapter9.AosukesSizeOfFeelingDayAfterTomorrow },
                };


                if(dataChapter9.InputChallengeText == "")
                {
                    excel.WriteCell($"A16", "");

                    string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter9/challenge_time_input_challege_event.isf");
                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"A16", $"challenge_time_input_challege_event.png", logDataPath, 20);
                    }
                }
                else
                {
                    chapter9StrResult.Add("A16",dataChapter9.InputChallengeText);
                }

                //CheckedNotGoodevent = 選択項目1,選択項目2,...,選択項目n,場面;の文字列となっている
                if (dataChapter9.CheckedNotGoodEvent != "")
                {
                    foreach (var notGoodEvents in dataChapter9.CheckedNotGoodEvent.Split(";").Select((Value, Index) => new { Value, Index }))
                    {
                        if (notGoodEvents.Value.Contains(","))
                        {
                            int cellColumn = 'B';
                            foreach (var notGoodEvent in notGoodEvents.Value.Split(",").Select((Value, Index) => new { Value, Index }))
                            {
                                cellColumn++;
                                if (notGoodEvent.Index != notGoodEvents.Value.Split(",").Length - 1)
                                {
                                    excel.DrawBorder($"{(char)cellColumn}{4 + notGoodEvents.Index}",false,false,false, true);
                                    chapter9StrResult.Add($"{(char)cellColumn}{4 + notGoodEvents.Index}", notGoodEvent.Value);
                                }
                            }
                        }
                    }
                }


                foreach (KeyValuePair<string, string> item in chapter9StrResult)
                {
                    if (item.Value.Contains(",良い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",良い", ""));
                    }
                    else if (item.Value.Contains(",悪い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",悪い", ""));
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);

                    }

                    //if (item.Value.Contains("●"))
                    //{
                    //    excel.WriteCell(item.Key, item.Value.Replace("●　", ""));
                    //}
                }

                foreach (KeyValuePair<string, int?> item in chapter9IntResult)
                {
                    if (item.Value == -1)
                    {
                        excel.WriteCell(item.Key, null);
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);
                    }
                }
                excel.AutoFitColumns();
            }

            //第10回に書き込み
            if (dataProgress.HasCompletedChapter10 == true)
            {
                excel.SetSheet("第10回");

                var chapter10StrResult = new Dictionary<string, string>
                {
                    { "B13", dataChapter10.AosukeChallengeStep1KindOfFeeling },
                    { "B12", dataChapter10.AosukeChallengeStep2KindOfFeeling },
                    { "B11", dataChapter10.AosukeChallengeStep3KindOfFeeling },
                };

                var chapter10IntResult = new Dictionary<string, int?>
                {
                    { "C13", dataChapter10.AosukeChallengeStep1SizeOfFeeling },
                    { "C12", dataChapter10.AosukeChallengeStep2SizeOfFeeling },
                    { "C11", dataChapter10.AosukeChallengeStep3SizeOfFeeling },
                };

                if(dataChapter10.ReasonDifferenceSizeOfFeelingInputText == "")
                {
                    excel.WriteCell($"A3", "");

                    string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter10/challenge_time_input_difference_reason.isf");
                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"A3", $"challenge_time_input_difference_reason.png", logDataPath, 30);
                    }
                }
                else
                {
                    chapter10StrResult.Add("A3",dataChapter10.ReasonDifferenceSizeOfFeelingInputText);
                }

                if (dataChapter10.AosukeSmallChallengeInputText == "")
                {
                    excel.WriteCell($"A6", "");

                    string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter10/challenge_time_input_aosuke's_small_challenge.isf");
                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"A6", $"challenge_time_input_aosuke's_small_challenge.png", logDataPath, 30);
                    }
                }
                else
                {
                    chapter10StrResult.Add("A6", dataChapter10.AosukeSmallChallengeInputText);
                }

                for (int i=0;i < 3; i++)
                {
                    string aosukeChallengeStepInputText = (string)typeof(DataChapter10).GetProperty($"AosukeChallengeStep{i+1}InputText").GetValue(dataChapter10);
                    if (aosukeChallengeStepInputText == "")
                    {
                        excel.WriteCell($"A{13-i}", "");

                        string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter10/challenge_time_input_aosuke's_challenge_step_{i+1}.isf");
                        if (File.Exists(logDataPath))
                        {
                            excel.PastePicture($"A{13 - i}", $"challenge_time_input_aosuke's_challenge_step_{i+1}.png", logDataPath, 30);
                        }
                    }
                    else
                    {
                        chapter10StrResult.Add($"A{13 - i}", aosukeChallengeStepInputText);
                    }
                }

                foreach (KeyValuePair<string, string> item in chapter10StrResult)
                {
                    if (item.Value.Contains(",良い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",良い", ""));
                    }
                    else if (item.Value.Contains(",悪い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",悪い", ""));
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);

                    }

                    //if (item.Value.Contains("●"))
                    //{
                    //    excel.WriteCell(item.Key, item.Value.Replace("●　", ""));
                    //}
                }

                foreach (KeyValuePair<string, int?> item in chapter10IntResult)
                {
                    if (item.Value == -1)
                    {
                        excel.WriteCell(item.Key, null);
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);
                    }
                }
                excel.AutoFitColumns();
            }

            //第11回に書き込み
            if (dataProgress.HasCompletedChapter11 == true)
            {
                excel.SetSheet("第11回");

                var chapter11StrResult = new Dictionary<string, string>
                {
                    { "B4", dataChapter11.SelectedItem },
                    { "B8", dataChapter11.SelectedAkamaruGoalText },
                };

                if(dataChapter11.AkamaruOtherSolutionUseItemInputText == "")
                {
                    excel.WriteCell($"A4", "");

                    string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter11/challenge_time_part_1_input_akamaru_other_solution.isf");
                    if (File.Exists(logDataPath))
                    {
                        excel.PastePicture($"A4", $"challenge_time_part_1_input_akamaru_other_solution.png", logDataPath, 30);
                    }
                }
                else
                {
                    chapter11StrResult.Add("A4",dataChapter11.AkamaruOtherSolutionUseItemInputText);
                }

                if (dataChapter11.Step2AkamaruManyMethodInputText =="")
                {
                    for (int i = 0; i < 3; i++)
                    {
                        excel.WriteCell($"A{8+i}", "");

                        string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter11/challenge_time_part_2_input_akamaru_method{i+1}.isf");
                        if (File.Exists(logDataPath))
                        {
                            excel.InsertCells($"A{8 + i}",ExcelManager.InsertMode.ShiftDown);
                            excel.DrawBorder($"A{8 + i}",true,true,true,true);
                            excel.PastePicture($"A{8+i}", $"challenge_time_part_2_input_akamaru_method{i + 1}.png", logDataPath, 50);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        chapter11StrResult.Add($"E{4 + i}", dataChapter11.Step2AkamaruManyMethodInputText.Split(",")[i]);
                    }
                }



                if (dataChapter11.EvaluationAkamaruMethodText1 != ""&&(dataChapter11.EvaluationAkamaruMethodText2 != "" && dataChapter11.EvaluationAkamaruMethodText3 !=""))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int cellColumn = 'B';

                        cellColumn += i;

                        chapter11StrResult.Add($"{(char)cellColumn}15", dataChapter11.EvaluationAkamaruMethodText1.Split(",")[i]);
                        chapter11StrResult.Add($"{(char)cellColumn}16", dataChapter11.EvaluationAkamaruMethodText2.Split(",")[i]);
                        chapter11StrResult.Add($"{(char)cellColumn}17", dataChapter11.EvaluationAkamaruMethodText3.Split(",")[i]);
                    }
                }
                

                if (dataChapter11.Step2AosukeManyMethodInputText=="")
                {
                    for (int i=0;i<6;i++)
                    {
                        excel.WriteCell($"A{21+i}", "");

                        string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter11/groupe_activity_input_aosuke's_method_{i+1}.isf");
                        if (File.Exists(logDataPath))
                        {
                            excel.DrawBorder($"A{21 + i}",true,true,true,true);
                            excel.PastePicture($"A{21 + i}", $"groupe_activity_input_aosuke's_method_{i + 1}.png", logDataPath, 40);
                        }
                    }
                }
                else
                {
                    foreach (var aosukeMethod in dataChapter11.Step2AosukeManyMethodInputText.Split(",").Select((Value, Index) => new { Value, Index }))
                    {
                        chapter11StrResult.Add($"A{21+aosukeMethod.Index}", aosukeMethod.Value);
                    }
                }


                if (dataChapter11.EvaluationAosukeMethodText !="")
                {
                    foreach (var evaluations in dataChapter11.EvaluationAosukeMethodText.Split(";").Select((Value, Index) => new { Value, Index }))
                    {
                        if (evaluations.Value !="")
                        {
                            foreach (var evaluation in evaluations.Value.Split(",").Select((Value, Index) => new { Value, Index }))
                            {
                                int cellColumn = 'B';
                                cellColumn += evaluation.Index;

                                excel.DrawBorder($"{(char)cellColumn}{4 + evaluations.Index}", true, true, true, true);
                                chapter11StrResult.Add($"{(char)cellColumn}{4 + evaluations.Index}", evaluation.Value);

                            }
                        }
                        
                    }
                }

                foreach (KeyValuePair<string, string> item in chapter11StrResult)
                {
                    if (item.Value.Contains(",良い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",良い", ""));
                    }
                    else if (item.Value.Contains(",悪い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",悪い", ""));
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);
                    }
                }

                excel.AutoFitColumns();

            }

            //第12回に書き込み
            if (dataProgress.HasCompletedChapter12 == true)
            {
                excel.SetSheet("第12回");

                var chapter12StrResult = new Dictionary<string, string>
                {

                    { "A4", dataChapter12.SelectedSceneText },
                    { "B4", dataChapter12.SelectedItem1 },
                    { "B5", dataChapter12.SelectedItem2 },
                    { "B6", dataChapter12.SelectedItem3 },
                };

                for(int i = 0; i < 3; i++)
                {
                    string itemMethodText = (string)typeof(DataChapter12).GetProperty($"ItemMethodInputText{i + 1}").GetValue(dataChapter12);
                    if(itemMethodText == "")
                    {
                        excel.WriteCell($"C{4 + i}", "");

                        string logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter12/groupe_activity_item_method_stroke{i + 1}.isf");
                        if (File.Exists(logDataPath))
                        {
                            excel.PastePicture($"C{4 + i}", $"groupe_activity_item_method_stroke{i + 1}.png", logDataPath, 40);
                        }
                    }
                    else
                    {
                        chapter12StrResult.Add($"C{4+i}",itemMethodText);
                    }
                }


                foreach (KeyValuePair<string, string> item in chapter12StrResult)
                {
                    if (item.Value.Contains(",良い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",良い", ""));
                    }
                    else if (item.Value.Contains(",悪い"))
                    {
                        excel.WriteCell(item.Key, item.Value.Replace(",悪い", ""));
                    }
                    else
                    {
                        excel.WriteCell(item.Key, item.Value);
                    }
                }

                excel.AutoFitColumns();
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

                var resultChapter1 = connection.Query<DataChapter1>($"SELECT * FROM 'DataChapter1' ORDER BY Id DESC LIMIT 1;");
                var typeOfDataChapter1 = dataChapter1.GetType();
                var dataChapterPropertyInfos1 = typeOfDataChapter1.GetProperties();
                if (resultChapter1.Count != 0)
                {
                    foreach (var dataChapterProp in dataChapterPropertyInfos1)
                    {
                        var dbProp = typeof(DataChapter1).GetProperty(dataChapterProp.Name);
                        if (dbProp.GetValue(resultChapter1[0]) != null)
                        {
                            dataChapterProp.SetValue(dataChapter1, dbProp.GetValue(resultChapter1[0]));
                        }
                    }
                }

                var resultChapter2 = connection.Query<DataChapter2>($"SELECT * FROM 'DataChapter2' ORDER BY Id DESC LIMIT 1;");
                var typeOfDataChapter2 = dataChapter2.GetType();
                var dataChapterPropertyInfos2 = typeOfDataChapter2.GetProperties();
                if (resultChapter2.Count != 0)
                {
                    foreach (var dataChapterProp in dataChapterPropertyInfos2)
                    {
                        var dbProp = typeof(DataChapter2).GetProperty(dataChapterProp.Name);
                        if (dbProp.GetValue(resultChapter2[0]) != null)
                        {
                            dataChapterProp.SetValue(dataChapter2, dbProp.GetValue(resultChapter2[0]));
                        }
                    }
                }

                var resultChapter3 = connection.Query<DataChapter3>($"SELECT * FROM 'DataChapter3' ORDER BY Id DESC LIMIT 1;");
                var typeOfDataChapter3 = dataChapter3.GetType();
                var dataChapterPropertyInfos3 = typeOfDataChapter3.GetProperties();
                if(resultChapter3.Count != 0)
                {
                    foreach (var dataChapterProp in dataChapterPropertyInfos3)
                    {
                        var dbProp = typeof(DataChapter3).GetProperty(dataChapterProp.Name);
                        if (dbProp.GetValue(resultChapter3[0]) != null)
                        {
                            dataChapterProp.SetValue(dataChapter3, dbProp.GetValue(resultChapter3[0]));
                        }
                    }
                }
                

                var resultChapter4 = connection.Query<DataChapter4>($"SELECT * FROM 'DataChapter4' ORDER BY Id DESC LIMIT 1;");
                var typeOfDataChapter4 = dataChapter4.GetType();
                var dataChapter4PropertyInfos = typeOfDataChapter4.GetProperties();
                if (resultChapter4.Count != 0)
                {
                    foreach (var dataChapterProp in dataChapter4PropertyInfos)
                    {
                        var dbProp = typeof(DataChapter4).GetProperty(dataChapterProp.Name);
                        if (dbProp.GetValue(resultChapter4[0]) !=null)
                        {
                            dataChapterProp.SetValue(dataChapter4, dbProp.GetValue(resultChapter4[0]));
                        }
                    }
                }

                var resultChapter5 = connection.Query<DataChapter5>($"SELECT * FROM 'DataChapter5' ORDER BY Id DESC LIMIT 1;");
                var typeOfDataChapter5 = dataChapter5.GetType();
                var dataChapter5PropertyInfos = typeOfDataChapter5.GetProperties();
                if (resultChapter5.Count != 0)
                {
                    foreach (var dataChapterProp in dataChapter5PropertyInfos)
                    {
                        var dbProp = typeof(DataChapter5).GetProperty(dataChapterProp.Name);
                        if (dbProp.GetValue(resultChapter5[0]) != null)
                        {
                            dataChapterProp.SetValue(dataChapter5, dbProp.GetValue(resultChapter5[0]));
                        }
                    }
                }

                var resultChapter6 = connection.Query<DataChapter6>($"SELECT * FROM 'DataChapter6' ORDER BY Id DESC LIMIT 1;");
                var typeOfDataChapter6 = dataChapter6.GetType();
                var dataChapterPropertyInfos6 = typeOfDataChapter6.GetProperties();
                if (resultChapter6.Count !=0)
                {
                    foreach (var dataChapterProp in dataChapterPropertyInfos6)
                    {
                        var dbProp = typeof(DataChapter6).GetProperty(dataChapterProp.Name);
                        if (dbProp.GetValue(resultChapter6[0]) != null)
                        {
                            dataChapterProp.SetValue(dataChapter6, dbProp.GetValue(resultChapter6[0]));
                        }
                    }
                }
                

                var resultChapter7 = connection.Query<DataChapter7>($"SELECT * FROM 'DataChapter7' ORDER BY Id DESC LIMIT 1;");
                var typeOfDataChapter7 = dataChapter7.GetType();
                var dataChapter7PropertyInfos = typeOfDataChapter7.GetProperties();
                if (resultChapter7.Count != 0)
                {
                    foreach (var dataChapterProp in dataChapter7PropertyInfos)
                    {
                        var dbProp = typeof(DataChapter7).GetProperty(dataChapterProp.Name);
                        if (dbProp.GetValue(resultChapter7[0]) != null)
                        {
                            dataChapterProp.SetValue(dataChapter7, dbProp.GetValue(resultChapter7[0]));
                        }
                    }
                }
                

                var resultChapter8 = connection.Query<DataChapter8>($"SELECT * FROM 'DataChapter8' ORDER BY Id DESC LIMIT 1;");
                var typeOfDataChapter8 = dataChapter8.GetType();
                var dataChapter8PropertyInfos = typeOfDataChapter8.GetProperties();
                if (resultChapter7.Count != 0)
                {
                    foreach (var dataChapterProp in dataChapter8PropertyInfos)
                    {
                        var dbProp = typeof(DataChapter8).GetProperty(dataChapterProp.Name);
                        if (dbProp.GetValue(resultChapter8[0]) != null)
                        {
                            dataChapterProp.SetValue(dataChapter8, dbProp.GetValue(resultChapter8[0]));
                        }
                    }
                }
                

                var resultChapter9 = connection.Query<DataChapter9>($"SELECT * FROM 'DataChapter9' ORDER BY Id DESC LIMIT 1;");
                var typeOfDataChapter9 = dataChapter9.GetType();
                var dataChapter9PropertyInfos = typeOfDataChapter9.GetProperties();
                if (resultChapter9.Count !=0)
                {
                    foreach (var dataChapterProp in dataChapter9PropertyInfos)
                    {
                        var dbProp = typeof(DataChapter9).GetProperty(dataChapterProp.Name);
                        if (dbProp.GetValue(resultChapter9[0]) != null)
                        {
                            dataChapterProp.SetValue(dataChapter9, dbProp.GetValue(resultChapter9[0]));
                        }
                    }
                }

                var resultChapter10 = connection.Query<DataChapter10>($"SELECT * FROM 'DataChapter10' ORDER BY Id DESC LIMIT 1;");
                var typeOfDataChapter10 = dataChapter10.GetType();
                var dataChapter10PropertyInfos = typeOfDataChapter10.GetProperties();
                if (resultChapter10.Count != 0)
                {
                    foreach (var dataChapterProp in dataChapter10PropertyInfos)
                    {
                        var dbProp = typeof(DataChapter10).GetProperty(dataChapterProp.Name);
                        if (dbProp.GetValue(resultChapter10[0]) != null)
                        {
                            dataChapterProp.SetValue(dataChapter10, dbProp.GetValue(resultChapter10[0]));
                        }
                    }
                }
               

                var resultChapter11 = connection.Query<DataChapter11>($"SELECT * FROM 'DataChapter11' ORDER BY Id DESC LIMIT 1;");
                var typeOfDataChapter11 = dataChapter11.GetType();
                var dataChapter11PropertyInfos = typeOfDataChapter11.GetProperties();
                if (resultChapter11.Count !=0)
                {
                    foreach (var dataChapterProp in dataChapter11PropertyInfos)
                    {
                        var dbProp = typeof(DataChapter11).GetProperty(dataChapterProp.Name);
                        if (dbProp.GetValue(resultChapter11[0]) != null)
                        {
                            dataChapterProp.SetValue(dataChapter11, dbProp.GetValue(resultChapter11[0]));
                        }
                    }
                }

                var resultChapter12 = connection.Query<DataChapter12>($"SELECT * FROM 'DataChapter12' ORDER BY Id DESC LIMIT 1;");
                var typeOfDataChapter12 = dataChapter12.GetType();
                var dataChapter12PropertyInfos = typeOfDataChapter12.GetProperties();
                if (resultChapter12.Count !=0)
                {
                    foreach (var dataChapterProp in dataChapter12PropertyInfos)
                    {
                        var dbProp = typeof(DataChapter12).GetProperty(dataChapterProp.Name);
                        if (dbProp.GetValue(resultChapter12[0]) != null)
                        {
                            dataChapterProp.SetValue(dataChapter12, dbProp.GetValue(resultChapter12[0]));
                        }
                    }
                }
            }
        }

        private void LoadDataChapter<TDataChapter>(TDataChapter _dataChapter,string dbPath)
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

                            if (dataChapterProp.Name =="MyKindOfGoodFeelings"|| dataChapterProp.Name == "MyKindOfBadFeelings")
                            {

                                //配列に直さないといけない場合
                            }
                            else if (false)
                            {
                                //手書き入力をロードする場合
                            }
                            else if (false)
                            {
                                //ListBoxに直さないといけない場合
                            }
                            else
                            {
                                dataChapterProp.SetValue(_dataChapter, dbProp.GetValue(resultChapter1));
                            }

                        }

                        break;
                    case "2":
                        var resultChapter2 = connection.Query<DataChapter2>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        break;
                    case "3":
                        var resultChapter3 = connection.Query<DataChapter3>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        break;
                    case "4":
                        var resultChapter4 = connection.Query<DataChapter4>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        break;
                    case "5":
                        var resultChapter5 = connection.Query<DataChapter5>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        break;
                    case "6":
                        var resultChapter6 = connection.Query<DataChapter1>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        break;
                    case "7":
                        var resultChapter7 = connection.Query<DataChapter1>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        break;
                    case "8":
                        var resultChapter8 = connection.Query<DataChapter1>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        break;
                    case "9":
                        var resultChapter9 = connection.Query<DataChapter1>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        break;
                    case "10":
                        var resultChapter10 = connection.Query<DataChapter1>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        break;
                    case "11":
                        var resultChapter11 = connection.Query<DataChapter1>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        break;
                    case "12":
                        var resultChapter12 = connection.Query<DataChapter1>($"SELECT * FROM '{typeOfDataChapter.Name}';");
                        break;
                }
            }

            

        }
    }
}

