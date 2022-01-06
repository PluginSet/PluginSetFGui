using System;
using System.Collections.Generic;
using System.IO;
using PluginSet.Core;
using UnityEditor;
using UnityEngine;

namespace PluginSet.FGUI.Editor
{
    public static class FairyGUIPathImport
    {
        private static readonly string FilePathReplaceFrom;
        private static readonly string FilePathReplaceTo;

        public delegate void WalkDirectoryFuncDelegate(string file, string bundleName);

        static FairyGUIPathImport()
        {
            FilePathReplaceFrom = Application.dataPath + Path.DirectorySeparatorChar;
            FilePathReplaceTo = "Assets" + Path.DirectorySeparatorChar;
        }
        
        public static void SetFairyGuiResourceBundles(IEnumerable<string> fguiPaths, string bundleNameFormatter)
        {
            foreach (var fguiPath in fguiPaths)
            {
                WalkDirectory(Path.Combine(Application.dataPath, fguiPath), bundleNameFormatter, SetFairyResourceBundleName);
            }
        }

        public static void ClearFairyGuiResourceBundles(IEnumerable<string> fguiPaths)
        {
            foreach (var fguiPath in fguiPaths)
            {
                WalkDirectory(Path.Combine(Application.dataPath, fguiPath), null, SetFairyResourceBundleName);
            }
        }
        

        public static void WalkDirectory(string dirPath, string bundleNameFormatter, WalkDirectoryFuncDelegate func)
        {
            var bundleName = bundleNameFormatter;
            if (bundleName != null)
                bundleName = GetBundleName(dirPath, bundleNameFormatter);
            
            foreach (var file in Directory.EnumerateFiles(dirPath, "*.*"))
            {
                if (IsValidResource(file))
                    func(file, bundleName);
            }

            foreach (var path in Directory.EnumerateDirectories(dirPath))
            {
                WalkDirectory(path, bundleNameFormatter, func);
            }
        }

        private static void SetFairyResourceBundleName(string file, string bundleName)
        {
            try
            {
                var importer = AssetImporter.GetAtPath(Global.GetAssetPath(file));
                importer.SetAssetBundleNameAndVariant(bundleName, null);
            }
            finally
            {
                
            }
        }

        private static string GetBundleName(string dirName, string bundleNameFormatter)
        {
            var name = Path.GetFileName(dirName);
            if (string.IsNullOrEmpty(bundleNameFormatter))
                return name;

            return string.Format(bundleNameFormatter, name);
        }

        private static bool IsValidResource(string file)
        {
            if (file.Contains(".DS_Store"))
                return false;

            if (file.EndsWith(".meta"))
                return false;

            return true;
        }
    }
}