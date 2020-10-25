using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace KoboldTools
{

    /// <summary>
    /// some file access helpers taken from kobold core version 2012. TODO: clean this up and update scripts
    /// </summary>
    public static class FileAccessHelpers
    {

        // Reads a specific file.
        public static List<string> readFile(string inPath)
        {
            //read file
            List<string> lines = new List<string>();
#if !UNITY_WEBPLAYER
            using (StreamReader sr = new StreamReader(inPath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
#endif
            return lines;
        }

        // Writes a specific file.
        public static void writeFile(List<string> inLines, string inPath)
        {
#if !UNITY_WEBPLAYER
            //write file
            using (StreamWriter sw = File.CreateText(inPath))
            {
                foreach (string line in inLines)
                {
                    sw.WriteLine(line);
                }
            }
#endif
        }

        // wirte to a config file
        public static string readFromConfigFile(string inPath, string entryName)
        {
            string value = "";
#if !UNITY_WEBPLAYER
            List<string> fileLines = FileAccessHelpers.readFile(inPath);
            for (int i = 0; i < fileLines.Count; i++)
            {
                string[] commands = fileLines[i].Split('=');
                if (commands[0].Equals(entryName))
                {
                    value = commands[1];
                }
            }
#endif
            return value;
        }

        // read from a config file
        public static void writeToConfigFile(string inPath, string entryName, string value)
        {
            bool entryFound = false;
#if !UNITY_WEBPLAYER
            List<string> fileLines = FileAccessHelpers.readFile(inPath);
            for (int i = 0; i < fileLines.Count; i++)
            {
                string[] commands = fileLines[i].Split('=');
                if (commands[0].Equals(entryName))
                {
                    fileLines[i] = entryName + "=" + value;
                    entryFound = true;
                }
            }
            if (!entryFound)
            {
                fileLines.Add(entryName + "=" + value);
            }
            //write file
            FileAccessHelpers.writeFile(fileLines, inPath);
#endif
        }

        public static void directoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
#if !UNITY_WEBPLAYER
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    directoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
#endif
        }
    }
}
