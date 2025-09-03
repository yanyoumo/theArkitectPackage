using System;
using System.Linq;
using I2.Loc;
using UnityEngine;

namespace theArkitectPackage.Editor
{
    public static class TextProcessHelper
    {
        public static bool CheckSharpMarksInPairs(string str)
        {
            //TODO
            return false;
            // var countofDQM = str.Count(c => c == '\"');
            // return countofDQM % 2 == 0;
        }
        
        public static bool CheckDoubleQuotationMarksInPairs(string str)
        {
            var countofDQM = str.Count(c => c == '\"');
            return countofDQM % 2 == 0;
        }
        
        public static bool CheckIllegalLineReturn(string str)
        {
            return !(str.Contains("\r") || str.Contains("\n") || str.Contains(Environment.NewLine));
        }
        
        public static bool CheckIllegalComma(string str)
        {
            return !str.Contains(",");
        }
        
        public static string GetCombineTickNameTrans(string MainTerm,string SubTerm)
        {
            var MainTitle = LocalizationManager.GetTranslation(MainTerm);
            var SubTitle = LocalizationManager.GetTranslation(SubTerm);
            return MainTitle + "<size=70%>" + SubTitle + "</size>";
        }
        
        // public static string StripIllegalLineReturn(string str)
        // {
        //     var un_returnContent = str;
        //     if (un_returnContent.Contains("\r")|| un_returnContent.Contains("\n"))
        //     {
        //         Debug.LogWarning(un_returnContent+"contain implicit return-line, which is forbidden, if needed use <br> instead.");
        //         un_returnContent = un_returnContent.Replace(Environment.NewLine, "<br>");
        //         un_returnContent = un_returnContent.Replace("\r", "<br>");
        //         un_returnContent = un_returnContent.Replace("\n", "<br>");
        //     }
        //     return un_returnContent;
        // }
        
        [Obsolete]
        public static string TmpColorBlueXml(string content)
        {
            return TmpColorXml(content, Color.blue);
        }
        [Obsolete]
        public static string TmpColorGreenXml(string content)
        {
            return TmpColorXml(content, Color.green * 0.35f);
        }
        [Obsolete]
        public static string TmpColorXml(string content, Color col)
        {
            var hexCol = ColorUtility.ToHtmlStringRGB(col);
            return "<color=#" + hexCol + ">" + content + "</color>";
        }
        [Obsolete]
        public static string TmpColorBold(string content)
        {
            return "<b>" + content + "</b>";
        }
        [Obsolete]
        public static string TmpBracket(string content)
        {
            return "[" + content + "]";
        }
        [Obsolete]
        public static string TmpBracketAndBold(string content) 
        {
            return TmpColorBold("[" + content + "]");
        }
        [Obsolete]
        public static string TMPNormalDataCompo()
        {
            return TmpBracketAndBold(TmpColorGreenXml("一般数据"));
        }
        [Obsolete]
        public static string TMPNetworkDataCompo()
        {
            return TmpBracketAndBold(TmpColorBlueXml("网络数据"));
        }
    }
}