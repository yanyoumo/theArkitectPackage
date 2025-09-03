using System.Collections.Generic;
using System.IO;
using System.Linq;
using I2.Loc;
using UnityEditor;
using UnityEngine;

namespace theArkitectPackage.Editor
{
    public static class i2LocWrapper
    {
        // private static LanguageSourceData MainLangSData => LocalizationManager.Sources[0];
        private static LanguageSourceData MainLangSData => Resources.Load<LanguageSourceAsset>("I2Languages").SourceData;

        public static string CheckNFixI2TermTextLegality(string str,string LogPostfix="")
        {
            if (!TextProcessHelper.CheckIllegalComma(str))
            {
                Debug.LogError("String "+str+" contain English comma please Fix!:" + LogPostfix);
                return "ORIGINAL WORD CONTAINS ILLEGAL COMMA";
            }
            
            if (!TextProcessHelper.CheckDoubleQuotationMarksInPairs(str))
            {
                Debug.LogError("String "+str+" contain not in pair DQM FIX!:" + LogPostfix);
                return "ORIGINAL WORD DQM NOT IN PAIR";
            }

            if (!TextProcessHelper.CheckIllegalLineReturn(str))
            {
                Debug.LogError("String "+str+" contain Illegal Line Return FIX!:" + LogPostfix);
                return "ORIGINAL WORD CONTAIN ILLEGAL LINE RETURN";
            }

            str = str.Replace("\"", "#@#");
            
            return str;
        }
        
        public static string[] ListAllComplexStoryTerms()
        {
            var res = new List<string>();
            string[] fileEntries = Directory.GetFiles(StaticName.LOC_ComplexStoryPath);
            foreach (string fileName in fileEntries)
            {
                var soloFileName = fileName.Split("\\").Last();
                // Debug.Log(soloFileName);
                if (soloFileName.Split(".").Last() != "meta" && soloFileName[0] == StaticName.LOC_ComplexStory_Prefix_FilterChar)
                {
                    //By pass all meta file and use LevelName as filter.
                    var deextensionFileName = soloFileName.Split(".").First();
                    // Debug.Log(deextensionFileName);
                    if (deextensionFileName.Split("_").Last() == StaticName.LOC_ComplexStory_LangPosfix_SimpChinese)
                    {
                        //use simplified Chinese Version as pivot.
                        var termName = deextensionFileName.Substring(0, deextensionFileName.Length - 3);
                        // Debug.Log(termName);
                        res.Add(termName);
                    }
                }
            }

            return res.ToArray();
        }

        public static bool CheckHasTerm(string TermName)
        {
            return MainLangSData.ContainsTerm(TermName);
        }
        
        private static void AddComicStoryDecoTerm(string TermName,string fileName)
        {
            MainLangSData.AddTerm("ComicDeco/"+TermName, eTermType.Sprite);
            var term = MainLangSData.GetTermData("ComicDeco/"+TermName);
            if (loadSpriteIntoI2(TermName + "_" + StaticName.LOC_ComplexStory_LangPosfix_SimpChinese))
            {
                term.SetTranslation(0, TermName + "_" + StaticName.LOC_ComplexStory_LangPosfix_SimpChinese);
            }
            if (loadSpriteIntoI2(TermName + "_" + StaticName.LOC_ComplexStory_LangPosfix_TradChinese))
            {
                term.SetTranslation(1, TermName + "_" + StaticName.LOC_ComplexStory_LangPosfix_TradChinese);
            }
            if (loadSpriteIntoI2(TermName + "_" + StaticName.LOC_ComplexStory_LangPosfix_English))
            {
                term.SetTranslation(2, TermName + "_" + StaticName.LOC_ComplexStory_LangPosfix_English);
            }

            bool loadSpriteIntoI2(string spriteName)
            {
                var t = (Sprite)AssetDatabase.LoadAssetAtPath(fileName+spriteName+".png", typeof(Sprite));
                if (t==null)
                {
                    return false;
                }
                if (MainLangSData.Assets.All(s => s.name != spriteName))
                {
                    MainLangSData.Assets.Add(t);
                }
                return true;
            }
        }
        
        public static void AddAllComicStoryDecoTerm()
        {
            var pathRoot = "Assets/Art/StoryComic";

            var folderFrontier = new Queue<string>();
            folderFrontier.Enqueue(pathRoot);

            var fileNames = new List<string>();

            do
            {
                var currentPath = folderFrontier.Dequeue();
                GetSubDirectoryOrFileNames(currentPath, out var subDirectories, out var subFileNames);
                foreach (var subDirectory in subDirectories)
                {
                    folderFrontier.Enqueue(subDirectory);
                }
                fileNames.AddRange(subFileNames);
            } while (folderFrontier.Count > 0);
            
            foreach (var fileEntry in fileNames)
            {
                var soloFileName = fileEntry.Split("\\").Last();
                if (soloFileName.Split(".").Last() != "meta")
                {
                    //By pass all meta file and use LevelName as filter.
                    var deExtensionFileName = soloFileName.Split(".").First();
                    // Debug.Log(deextensionFileName);
                    if (deExtensionFileName.Split("_").Last() == StaticName.LOC_ComplexStory_LangPosfix_SimpChinese)
                    {
                        //use simplified Chinese Version as pivot.
                        var termName = deExtensionFileName.Split("_")[0];
                        AddComicStoryDecoTerm(termName, fileEntry.Split(termName)[0]);
                        // Debug.Log(fileEntry.Split(termName)[0]);
                        // Debug.Log(termName);
                    }
                }
            }

            MainLangSData.UpdateDictionary();
            
            void GetSubDirectoryOrFileNames(string path,out string[] subDirectories,out string[] fileNames)
            {
                subDirectories = Directory.GetDirectories(path).ToArray();
                fileNames = Directory.GetFiles(path).ToArray();
            }
        }
        
        public static void AddAllComplexStoryTerm()
        {
            foreach (var complexStoryTerm in ListAllComplexStoryTerms())
            {
                AddComplexStoryTerm(complexStoryTerm);
            }
            MainLangSData.UpdateDictionary();
        }

        // public static void AddComplexStoryTerm(string TermRawName)
        // {
        //     var termName = StaticName.LOC_ComplexStory_CategoryPrefix + TermRawName;
        //     MainLangSData.AddTerm(termName, eTermType.GameObject);
        //     var term = MainLangSData.GetTermData(termName);
        //     //这里是很神奇的，i2使用的格式。就不要用path.combine了。
        //     term.SetTranslation(0, StaticName.LOC_ComplexStory_FileDirPrefix + TermRawName + "_"+StaticName.LOC_ComplexStory_LangPosfix_SimpChinese);
        //     term.SetTranslation(1, StaticName.LOC_ComplexStory_FileDirPrefix + TermRawName + "_"+StaticName.LOC_ComplexStory_LangPosfix_TradChinese);
        //     term.SetTranslation(2, StaticName.LOC_ComplexStory_FileDirPrefix + TermRawName + "_"+StaticName.LOC_ComplexStory_LangPosfix_English);
        // }
        
        private static void AddPrefabResTerm(string TermName)
        {
            MainLangSData.AddTerm(TermName, eTermType.GameObject);
            var term = MainLangSData.GetTermData(TermName);
            term.SetTranslation(0, TermName + "_" + StaticName.LOC_ComplexStory_LangPosfix_SimpChinese);
            term.SetTranslation(1, TermName + "_" + StaticName.LOC_ComplexStory_LangPosfix_TradChinese);
            term.SetTranslation(2, TermName + "_" + StaticName.LOC_ComplexStory_LangPosfix_English);
        }

        public static void AddComplexStoryTerm(string TermRawName)
        {
            AddPrefabResTerm(StaticName.LOC_ComplexStory_FileDirPrefix + TermRawName);
        }

        public static string AddWikiPanelLocTerm(string TermRawName)
        {
            Debug.Log("AddWikiPanelLocTerm");
            var TermName = StaticName.LOC_WikiPage_FileDirPrefix + TermRawName + "_" + StaticName.LOC_WikiPage_PanelLoc;
            if (!CheckHasTerm(TermName))
            {
                AddPrefabResTerm(TermName);
            }
            return TermName;
        }
        
        public static void ReplaceLocalizationCSV(string CSVfileContent)
        {
            MainLangSData.Import_CSV(string.Empty, CSVfileContent, eSpreadsheetUpdateMode.Replace, ',');
            LocalizationManager.LocalizeAll(); // Force localing all enabled labels/sprites with the new data
        }

        public static void AddLocalizationCSV(string CSVfileContent)
        {
            MainLangSData.Import_CSV(string.Empty, CSVfileContent, eSpreadsheetUpdateMode.AddNewTerms);
            LocalizationManager.LocalizeAll();
        }

        private static Dictionary<string, string> TermName_CategoryLib = new()
        {
            { "LevelName", StaticName.LOC_Tutorial_CategoryPrefix },
        };

        public static void CategorizeI2Terms()
        {
            var allTermListRaw = MainLangSData.GetTermsList().ToArray(); //Copy old key data.
            foreach (var s in allTermListRaw)
            {
                var termData = MainLangSData.GetTermData(s);
                if (termData.TermType == eTermType.Text)
                {
                    var termName = termData.Term;
                    if (TermName_CategoryLib.Keys.Any(termName.Contains))
                    {
                        var key = TermName_CategoryLib.Keys.First(termName.Contains);
                        var newTermName = TermName_CategoryLib[key] + termName;
                        termData.Term = newTermName;
                        
                        //TODO 他这里没有独立的API，这个到时候早晚得自己写。
                        // MainLangSData.AddTerm(termData);
                        // source.RemoveTerm(Term.Term);
                        // Term.Languages = localizations;

                        LocalizationManager.UpdateSources();
                    }
                }
            }
        }

        public static void RemoveAllTextTerms()
        {
            var allTermListRaw = MainLangSData.GetTermsList().ToArray();//Copy old key data.
            foreach (var s in allTermListRaw)
            {
                if (MainLangSData.GetTermData(s).TermType == eTermType.Text)
                {
                    MainLangSData.RemoveTerm(s);
                }
            }
        }
        
        public static void RemoveAllComplexStoryTerms_Simple()
        {
            //这里手动做一个判断，Demo版本专用。Master版本更有更完善的流程搞这个。
            var allTermListRaw = MainLangSData.GetTermsList().ToArray();//Copy old key data.
            foreach (var s in allTermListRaw)
            {
                if (MainLangSData.GetTermData(s).TermType == eTermType.GameObject)
                {
                    if (MainLangSData.GetTermData(s).Term.StartsWith("LevelName"))
                    {
                        MainLangSData.RemoveTerm(s);
                    }
                }
            }
        }
            
        public static void RemoveAllComplexStoryTerms_ComicDeco()
        {
            //这里手动做一个判断，Demo版本专用。Master版本更有更完善的流程搞这个。
            var allTermListRaw = MainLangSData.GetTermsList().ToArray();//Copy old key data.
            foreach (var s in allTermListRaw)
            {
                if (MainLangSData.GetTermData(s).TermType == eTermType.GameObject)
                {
                    if (MainLangSData.GetTermData(s).Term.StartsWith("ComicDeco"))
                    {
                        MainLangSData.RemoveTerm(s);
                    }
                }
            }
        }
        
        public static void RemoveAllGameObjectTerms()
        {
            var allTermListRaw = MainLangSData.GetTermsList().ToArray();//Copy old key data.
            foreach (var s in allTermListRaw)
            {
                if (MainLangSData.GetTermData(s).TermType == eTermType.GameObject)
                {
                    MainLangSData.RemoveTerm(s);
                }
            }
        }
    }
}