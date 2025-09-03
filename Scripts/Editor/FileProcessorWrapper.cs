using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace theArkitectPackage.Editor
{
    public static class FileProcessorWrapper
    {
        #region ProcessAllFileWithinSubFolder

        public static bool FileHasExtensionName(string path,string ext)
        {
            if (!ext.Contains('.'))
            {
                ext = "." + ext;
            }
            return path.ToLower().Contains(ext.ToLower());
        }
        
        private static void ProcessDirectory(string targetDirectory,Action<string> Processor,List<string> whiteList) 
        {
            // Process the list of files found in the directory.
            string [] fileEntries = Directory.GetFiles(targetDirectory);
            foreach(string fileName in fileEntries)
                ProcessFile(fileName,Processor,whiteList);

            // Recurse into subdirectories of this directory.
            string [] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach(string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory,Processor,whiteList);
        }
    
        private static void ProcessFile(string path,Action<string> Processor,IReadOnlyCollection<string> whiteList)//Bypass meta.
        {
            if (FileHasExtensionName(path, "meta"))
            {
                return;
            }

            if (whiteList.Count!=0 && !whiteList.Any(e=>FileHasExtensionName(path, e)))
            {
                return;
            }
            
            Processor(path);
        }
        
        public static void ProcessAllFileWithinSubFolder(string RootString,Action<string> Processor,List<string> whiteList)
        {
            if(File.Exists(RootString)) 
            {
                // This path is a file
                ProcessFile(RootString, Processor, whiteList);
            }               
            else if(Directory.Exists(RootString)) 
            {
                // This path is a directory
                ProcessDirectory(RootString, Processor, whiteList);
            }
            else
            {
                Debug.LogError(RootString + " is not a valid file or directory.");
            }
        }
        
        #endregion
    }
}