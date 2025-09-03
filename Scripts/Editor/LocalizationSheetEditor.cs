using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace theArkitectPackage.Editor
{
    public static class StaticName
    {
        public static string RootRepoPath => Application.dataPath.Substring(0, Application.dataPath.Length - 17);
        public const string DATE_TIME_CHINA_TIMEZONE_ID = "China Standard Time";
        
        public static string APP_DATAPATH => Application.dataPath;
        public static string PLAYER_APPDATA => Application.persistentDataPath;
        public static string PLAYER_APPDATA_COMPANY => Directory.GetParent(Application.persistentDataPath)?.FullName;

        public const string LOC_RootPath = "Assets/Editor/Root_Localization";
        public const string LOC_RootPath_Total = "Assets/Editor/Root_Localization_TotalFile.csv";
        public const string LOC_RootPath_TmpTotal = "Assets/Editor/Root_Localization_TotalFile_Tmp.csv";
        public const string LOC_ComplexStoryPath = "Assets/Resources/ComplexStory";

        public const string LOC_TermAlt_Suffix_PC = "";//需要留空，方便算法
        public const string LOC_TermAlt_Suffix_Contoller = "_controller";
        public const string LOC_TermAlt_Suffix_Mouse = "_mouse";
        public const string LOC_Suffix_LevelSimpleStory = "_Story";
        public const string LOC_Suffix_LevelComplexStory = "_ComplexStory";
        public const string LOC_Suffix_LevelDetail = "_Detail";
        public const string LOC_Suffix_LevelThumbnail = "_THB";
        public const string LOC_Suffix_Remake = "_Remake";
        public const string LOC_Title_Suffix_Remake = "_Rmk";

        public const string LOC_ComplexStory_FileDirPrefix = "ComplexStory/";
        public const string LOC_ComplexStory_CategoryPrefix = "ComplexStory/";
        public const string LOC_Tutorial_CategoryPrefix = "Tutorial/";
        public const char LOC_ComplexStory_Prefix_FilterChar = 'L';

        public const string LOC_WikiPage_FileDirPrefix = "WikiPage/";
        public const string LOC_WikiPage_PanelLoc = "PanelLoc";
        
        public const string LOC_ComplexStory_LangPosfix_SimpChinese = "zh";
        public const string LOC_ComplexStory_LangPosfix_TradChinese = "zht";
        public const string LOC_ComplexStory_LangPosfix_English = "en";
    }
    
    public sealed class LocalizationSheetEditor
    {
        private void DisplayFilePath(string path)
        {
            Debug.Log(path);
        }

        private void CheckHasDoubleDQM_OtherThanEmptyString_Debug(string targetPath)
        {
            if (CheckHasDoubleDQM_OtherThanEmptyString(targetPath))
            {
                Debug.LogError(targetPath);
            }
        }

        private bool CheckHasDoubleDQM_OtherThanEmptyString(string targetPath)
        {
            StreamReader sr;
            try
            {
                sr = new StreamReader(targetPath, Encoding.UTF8);
            }
            catch (IOException)
            {
                Debug.LogError("目标文件正在被其他软件（Excel）打开，需要先关闭！！");
                return false;
            }
            var header = sr.ReadLine();//Header;这个行目前先读掉不要。
            do
            {
                // Debug.Log(sr.ReadLine());
                var currentLine = sr.ReadLine();
                var deEmptyStrlst = currentLine.Split(",\"\",");
                var deEmptyStr = deEmptyStrlst.Aggregate("", (current, s) => current + (s + "@"));
                deEmptyStrlst = deEmptyStr.Split("=\"\"");
                deEmptyStr = deEmptyStrlst.Aggregate("", (current, s) => current + (s + "@"));
                deEmptyStrlst = deEmptyStr.Split("\"\">");
                deEmptyStr = deEmptyStrlst.Aggregate("", (current, s) => current + (s + "@"));
                deEmptyStrlst = deEmptyStr.Split("\"\" ");
                deEmptyStr = deEmptyStrlst.Aggregate("", (current, s) => current + (s + "@"));
                if (deEmptyStr.Contains("\"\""))
                {
                    Debug.LogWarning(currentLine);
                    sr.Close();
                    return true;
                }
            } while (!sr.EndOfStream);
            sr.Close();
            return false;
        }

        private void WriteFromListToTarget(List<string> scrContent,string targetPath)
        {
            // StreamReader sr;
            StreamWriter sw;
            try
            {
                // sr = new StreamReader(path, Encoding.UTF8);
                sw = new StreamWriter(targetPath, false, Encoding.UTF8);
            }
            catch (IOException)
            {
                Debug.LogError("目标文件正在被其他软件（Excel）打开，需要先关闭！！");
                return;
            }
            
            var title = string.Join(",", "Key", "Type", "Desc", "Chinese (Simplified)", "Chinese (Traditional)", "English", "");
            sw.WriteLine(title);
            
            foreach (var s in scrContent)
            {
                var str = s;
                str = str.Replace("#@#", "\"");
                sw.WriteLine(str);
            }
            sw.Close();
        }

        
        private List<string> ExtractTotalCSVAsList()
        {
            StreamReader sr;
            try
            {
                sr = new StreamReader(StaticName.LOC_RootPath_Total, Encoding.UTF8);
            }
            catch (IOException)
            {
                Debug.LogError("目标文件正在被其他软件（Excel）打开，需要先关闭！！");
                return new List<string>();
            }
            var TotalcsvByLine = new List<string>();

            var header = sr.ReadLine();//Header;这个行目前先读掉不要。
            do
            {
                // Debug.Log(sr.ReadLine());
                var currentLine = sr.ReadLine();
                TotalcsvByLine.Add(currentLine);
            } while (!sr.EndOfStream);
            sr.Close();
            return TotalcsvByLine;
        }
        
        private int SearchSingleTermIndexFromTotalCSV(string Term, List<string> TotalcsvByLine)
        {
            if (!TotalcsvByLine.Any(s => s.StartsWith(Term)))
            {
                return -1;
            }
            var TermLine = TotalcsvByLine.FirstOrDefault(s => s.StartsWith(Term));
            return TotalcsvByLine.FindIndex(s => s.Equals(TermLine));
        }
        
        public void SearchAndReplaceTermFromTotalCSV(string path,List<string> TotalcsvByLine)
        {
            var lineSearchRange = 5;
            StreamReader sr;
            try
            {
                sr = new StreamReader(path, Encoding.UTF8);
            }
            catch (IOException)
            {
                Debug.LogError("目标文件正在被其他软件（Excel）打开，需要先关闭！！");
                return;
            }
            var header = sr.ReadLine();//Header;这个行目前先读掉不要。
            var firstLineTerm = sr.ReadLine();
            var elements = firstLineTerm.Split(",");

            //理论上还要处理head都找不到情况，但是估计不存在。
            var headIndex = SearchSingleTermIndexFromTotalCSV(elements[0], TotalcsvByLine);

            if (headIndex==-1)
            {
                Debug.LogError(path+" Header is not found in Total");
                sr.Close();
                return;
            }
            
            // Debug.Log(TotalcsvByLine[headIndex]);
            var newLocalFileAsList = new List<string> { TotalcsvByLine[headIndex] };

            if (!sr.EndOfStream)
            {
                do
                {
                    var currentLineOffset = 0;
                    headIndex++;
                    var currentLine = sr.ReadLine();
                    var currentElements = currentLine.Split(",");
                    var totalLine = TotalcsvByLine[headIndex];
                    while (!totalLine.StartsWith(currentElements[0]))
                    {
                        currentLineOffset++;
                        totalLine = TotalcsvByLine[headIndex + currentLineOffset];
                        if (currentLineOffset > lineSearchRange)
                        {
                            headIndex--;//这里头指针需要“回退1”，因为相当目前的这个内容不是所需求的。所以指针其实是不能前进。
                            Debug.LogError(path + " Term:" + currentElements[0] + " is not found in Total");
                            totalLine = currentLine;
                            break;
                        }
                    }

                    if (totalLine != currentLine)
                    {
                        headIndex += currentLineOffset;
                    }
                    newLocalFileAsList.Add(totalLine);
                    
                } while (!sr.EndOfStream);
            }
            else
            {
                Debug.LogWarning(path + "Only has one Term");
            }
            sr.Close();
            WriteFromListToTarget(newLocalFileAsList, path);
        }
        
        // [Button]
        public void ValidateAndFixTermsFromCSV(string path="K:\\ROOT_Unity_Project\\R.O.O.T_Unity\\ROOT_demo\\Assets\\Editor\\Root_Localization\\ROOT_Localization_LevelDetail.csv",bool doFix=false)
        {
            StreamReader sr;
            StreamWriter sw;
            try
            {
                sr = new StreamReader(path, Encoding.UTF8);
                sw = new StreamWriter(path + "_tmp", false, Encoding.UTF8);
            }
            catch (IOException)
            {
                Debug.LogError("目标文件正在被其他软件（Excel）打开，需要先关闭！！");
                return;
            }

            var header = sr.ReadLine();//Header;这个行目前先读掉不要。
            if (header != null && header.Last()!=',')
            {
                header += ",";
            }
            sw.WriteLine(header);

            do
            {
                // Debug.Log(sr.ReadLine());
                var currentLine = sr.ReadLine();
                var fixedLines = "";
                
                if (!TextProcessHelper.CheckDoubleQuotationMarksInPairs(currentLine))
                {
                    //这里是不凑对引号的问题。这里原则上应该彻底退掉，因为后面没法处理。
                    Debug.LogError("String "+currentLine+" contain not in pair DQM FIX!, NOT fixable by script");
                    sr.Close();
                    sw.Close();
                    return;
                }
                
                // Debug.Assert(currentLine.Last()==Environment.NewLine);//这里是没有疑问的。

                var elements = currentLine.Split(",");

                var stringIdxStart = 3;

                // var elementsToProcessCount = elements.Length;
                
                Debug.Assert(elements.Length>=6,"this chart missing column");//这里是没有疑问的。

                for (var i = 0; i < 6; i++)
                {
                    var element = elements[i];
                    if (i < stringIdxStart)
                    {
                        //SubHeader.
                        fixedLines += element + ",";
                        continue;//目前不处理。
                    }

                    if (!TextProcessHelper.CheckDoubleQuotationMarksInPairs(element))
                    {
                        var subElement = element;
                        do
                        {
                            i++;
                            subElement += "," + elements[i];
                        } while (!TextProcessHelper.CheckDoubleQuotationMarksInPairs(subElement));
                        element = subElement;
                        //现在才拿到凑对的独立元素。
                    }

                    // Debug.Log("element=" + element);
                    if (string.IsNullOrEmpty(element)||(element.First() != '\"' || element.Last() != '\"'))
                    {
                        //说明这个独立元素虽然引号凑对，但是，并没有外围引号。
                        Debug.LogError("String "+currentLine+" contain not DQMed string, fixable by script");
                        fixedLines += "\"" + element + "\",";
                    }
                    else
                    {
                        fixedLines += element + ",";
                    }
                }

                if (currentLine.Last()!=',')
                {
                    //本行不是由逗号结尾，应该加上。fixLine在构建的时候自然会加上结尾逗号。
                }
                
                sw.WriteLine(doFix ? fixedLines : currentLine);
            } while (!sr.EndOfStream);

            sr.Close();
            sw.Close();
            File.Delete(path);
            File.Move(path + "_tmp", path);
        }

        void ReadCSVFrom_Resources_AddTerm(string path)
        {
            try
            {
                i2LocWrapper.AddLocalizationCSV(File.ReadAllText(path));
            }
            catch (IOException)
            {
                Debug.LogError("目标文件正在被其他软件（Excel）打开，需要先关闭！！");
            }
        }

        void CountAllTermsCharacters(string path)
        {
            try
            {
                var csvAsString = File.ReadAllText(path);
                var splited = csvAsString.Split(new[] { ",", "\n" }, StringSplitOptions.None);
                for (var i = 0; i < splited.Length; i++)
                {
                    if (i > 5)
                    {
                        if ((i - 3) % 6 == 0)
                        {
                            Debug.Log("TermVerb=" + splited[i] + "@" + i);
                            tempCounter += splited[i].Length;
                        }
                    }
                }
            }
            catch (IOException)
            {
                Debug.LogError("目标文件正在被其他软件（Excel）打开，需要先关闭！！");
            }
        }

        [ShowInInspector] public string ROOT_LocalizationRootPath => StaticName.LOC_RootPath;

        private int tempCounter=0;
        
        [Button]
        public void CountCharAllTermsFromCSV()
        {
            // var number;
            tempCounter = 0;
            // FileProcessorWrapper.ProcessAllFileWithinSubFolder(ROOT_LocalizationRootPath, CountAllTermsCharacters, new List<string> { "csv" });
            var srcData = Resources.Load<LanguageSourceAsset>("I2Languages");
            foreach (var s in srcData.SourceData.GetTermsList())
            {
                var t=srcData.SourceData.GetTranslation(s);
                tempCounter += t.Length;
                Debug.Log(t);
            }
            Debug.Log("All characters count=" + tempCounter);
        }

        private void CopyNPasteCSVToTarget(string path,string targetPath,bool doReplace=true)
        {
            StreamReader sr;
            StreamWriter sw;
            try
            {
                sr = new StreamReader(path, Encoding.UTF8);
                sw = new StreamWriter(targetPath, true, Encoding.UTF8);
            }
            catch (IOException)
            {
                Debug.LogError("目标文件正在被其他软件（Excel）打开，需要先关闭！！");
                return;
            }
            
            sr.ReadLine();//Header;这个行目前先读掉不要。

            do
            {
                var str = sr.ReadLine();
                if (doReplace)
                {
                    str = str.Replace("#@#", "\"");
                }
                sw.WriteLine(str);
            } while (!sr.EndOfStream);

            sr.Close();
            sw.Close();
        }
        
        // [Button]
        public void ValidateAllTermsFromCSV()
        {
            FileProcessorWrapper.ProcessAllFileWithinSubFolder(ROOT_LocalizationRootPath, s=>ValidateAndFixTermsFromCSV(s), new List<string> { "csv" });
        }
        
        // [Button]
        public void FixAllTermsFromCSV()
        {
            FileProcessorWrapper.ProcessAllFileWithinSubFolder(ROOT_LocalizationRootPath, s=>ValidateAndFixTermsFromCSV(s,true), new List<string> { "csv" });
        }

        public void WriteAllCSVToWriteableFile()
        {
            WriteAllCSVToWriteableFile_Core(StaticName.LOC_RootPath_Total, false);
        }
        
        public void WriteAllCSVToWriteableFile_Tmp()
        {
            WriteAllCSVToWriteableFile_Core(StaticName.LOC_RootPath_TmpTotal);
        }

        // [Button]
        public void WriteAllCSVToWriteableFile_Core(string targetPath,bool doReplace=true)
        {
            StreamWriter sw;
            try
            {
                sw = new StreamWriter(targetPath, false, Encoding.UTF8);
            }
            catch (IOException)
            {
                Debug.LogError("目标文件正在被其他软件（Excel）打开，需要先关闭！！");
                return;
            }

            var title = string.Join(",", "Key", "Type", "Desc", "Chinese (Simplified)", "Chinese (Traditional)", "English", "");
            sw.WriteLine(title);
            
            sw.Close();
            FileProcessorWrapper.ProcessAllFileWithinSubFolder(ROOT_LocalizationRootPath, s=>CopyNPasteCSVToTarget(s,targetPath,doReplace), new List<string> { "csv" });
        }
        
        // private List<string> FileNameAsList;
        //
        // void ReadCSVFrom_Resources_AddNameToList(string path,List<string> list)
        // {
        //     list.Add(path);
        // }

        // private void SaveAllCSVNameToList()
        // {
        //     FileNameAsList = new List<string>();
        //     FileProcessorWrapper.ProcessAllFileWithinSubFolder(ROOT_LocalizationRootPath, s=>ReadCSVFrom_Resources_AddNameToList(s,FileNameAsList), new List<string> { "csv" });
        //     // for (var i = 0; i < FileNameAsList.Count; i++)
        //     // {
        //     //     EditorUtility.DisplayProgressBar("Simple Progress Bar", "Doing some work...", i / (float)FileNameAsList.Count);
        //     //     Debug.Log(FileNameAsList[i]);
        //     // }
        //     // EditorUtility.ClearProgressBar();
        // }

        // [Button]
        // public void ReplaceAllTermsFromCSVwProgressBar()
        // {
        //     i2LocWrapper.RemoveAllTextTerms();
        //     SaveAllCSVNameToList();
        //     
        //     for (var i = 0; i < FileNameAsList.Count; i++)
        //     {
        //         EditorUtility.DisplayProgressBar("Simple Progress Bar", "Doing some work...", i / (float)FileNameAsList.Count);
        //         try
        //         {
        //             i2LocWrapper.AddLocalizationCSV(File.ReadAllText(FileNameAsList[i]));
        //         }
        //         catch (IOException)
        //         {
        //             Debug.LogError("目标文件正在被其他软件（Excel）打开，需要先关闭！！");
        //         }
        //     }
        //     
        //     EditorUtility.SetDirty(Resources.Load<LanguageSourceAsset>("I2Languages"));
        //     AssetDatabase.SaveAssets();
        //     EditorUtility.ClearProgressBar();
        // }

        // private IEnumerator RootProgressBarCor()
        // {
        //     do
        //     {
        //         
        //     } while ();
        //     EditorUtility.ClearProgressBar();
        // }
        //
        // [Button]
        // public void RootProgressBarTest()
        // {
        //     // EditorUtility.DisplayProgressBar("Simple Progress Bar", "Doing some work...", t / secs);
        //     
        //     EditorUtility.ClearProgressBar();
        // }
        [PropertySpace]
        [Button]
        public void ExportAllCSVTermsToTotal()
        {
            FixAllTermsFromCSV();
            WriteAllCSVToWriteableFile();
        }

        // [Button]
        public void ReplaceAllTermsFromCSVTotal()
        {
            i2LocWrapper.RemoveAllTextTerms();
            ReadCSVFrom_Resources_AddTerm(StaticName.LOC_RootPath_Total);
            EditorUtility.SetDirty(Resources.Load<LanguageSourceAsset>("I2Languages"));
            AssetDatabase.SaveAssets();
        }
        [Button]
        public void SearchAndReplaceAllTermFromTotalCSV()
        {
            var TotalCSVAsList = ExtractTotalCSVAsList();
            FileProcessorWrapper.ProcessAllFileWithinSubFolder(ROOT_LocalizationRootPath, s=>SearchAndReplaceTermFromTotalCSV(s,TotalCSVAsList), new List<string> { "csv" });
            FixAllTermsFromCSV();
        }

        [PropertySpace]
        [Button]
        public void ReplaceAllTermsFromCSV()
        {
            FixAllTermsFromCSV();
            WriteAllCSVToWriteableFile_Tmp();
            i2LocWrapper.RemoveAllTextTerms();
            ReadCSVFrom_Resources_AddTerm(StaticName.LOC_RootPath_TmpTotal);
            File.Delete(StaticName.LOC_RootPath_TmpTotal);
            EditorUtility.SetDirty(Resources.Load<LanguageSourceAsset>("I2Languages"));
            AssetDatabase.SaveAssets();
        }
        
        [PropertySpace]
        [Button]
        public void ReplaceAllComplexStoryFromRES()
        {
            i2LocWrapper.RemoveAllComplexStoryTerms_Simple();
            i2LocWrapper.AddAllComplexStoryTerm();
            EditorUtility.SetDirty(Resources.Load<LanguageSourceAsset>("I2Languages"));
            AssetDatabase.SaveAssets();
        }

        [PropertySpace]
        [Button]
        public void ReplaceAllStoryComicDecoLoc()
        {
            i2LocWrapper.RemoveAllComplexStoryTerms_ComicDeco();
            i2LocWrapper.AddAllComicStoryDecoTerm();
            EditorUtility.SetDirty(Resources.Load<LanguageSourceAsset>("I2Languages"));
            AssetDatabase.SaveAssets();
        }

        [PropertySpace]
        [Button]
        public void CheckAllCSVFromDoubleDQM()
        {
            FileProcessorWrapper.ProcessAllFileWithinSubFolder(ROOT_LocalizationRootPath, CheckHasDoubleDQM_OtherThanEmptyString_Debug, new List<string> { "csv" });
        }
    }
}