using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
// Description: Patch Log generator, to be clear, is generating a file telling patch tool how to build a patch.
//              It compares two files with same name from two selected standalone folders on thier checksum. 
//              Patch Log will log files that need to update, remove, or create.
//////////////////////////////////////
namespace iMVR_Launcher
{
    public static class PatchLogGenerator
    {
        public static void GeneratePatchFromTwoStandAlone(string[] files1, string[] files2, string[] dir1, string[] dir2, string version,
            string path_OldStandAlone, string path_NewStandAlone, ref float progressStat, string PatchPath, Action<string> OnComplete = null)
        {
            PatchGenerate(DifferAllFiles(files1, files2, dir1, dir2, version, path_OldStandAlone, path_NewStandAlone, progressStat: ref progressStat), PatchPath, OnComplete);
        }

        public static PatchInfo DifferAllFiles(string[] files1, string[] files2, string[] dir1, string[] dir2, string version, 
            string path_OldStandAlone, string path_NewStandAlone, ref float progressStat)
        {
            MD5Lib md5_1 = new MD5Lib();
            MD5Lib md5_2 = new MD5Lib();
            FilesInfo md_path1 = new FilesInfo();
            FilesInfo md_path2 = new FilesInfo();
            md5_1 = AddToDictionary(md5_1, files1, dir1, path_OldStandAlone.Length, md_path1, ref progressStat);
            md5_2 = AddToDictionary(md5_2, files2, dir2, path_NewStandAlone.Length, md_path2, ref progressStat);
            return DictionaryCompare(md5_1, md5_2, version, path_OldStandAlone);
        }

        public static MD5Lib AddToDictionary(MD5Lib md5, string[] files, string[] dirs, int length, FilesInfo path, ref float progressStat)
        {
            float counter = 0;
            try
            {
                foreach (string filename in files)
                {
                    string file = filename.Substring(length).ToString().Replace(@"\", "/");
                    path.FilePath = filename.ToString().Replace(@"\", "/");
                    path.FileMD5 = FileHelper.FindMD5(filename);
                    md5.Files.Add(file, path);
                    progressStat = counter / files.Length;
                    counter++;
                }

                foreach (string dir in dirs)
                {
                    string subdir = dir.Substring(length).ToString().Replace(@"\", "/");
                    path.FilePath = dir.ToString().Replace(@"\", "/");
                    path.FileMD5 = "";
                    md5.Files.Add(subdir, path);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
            return md5;
        }

        public static PatchInfo DictionaryCompare(MD5Lib lib1, MD5Lib lib2, string version, string path_OldStandAlone)
        {
            PatchInfo patch = new PatchInfo
            {
                patchversion = version
            };

            foreach (KeyValuePair<string, FilesInfo> item in lib2.Files)
            {
                if (lib1.Files.ContainsKey(item.Key) && item.Value.FileMD5 != lib1.Files[item.Key].FileMD5)
                {
                    patch.filesToReplace.Add(item.Value.FilePath, lib1.Files[item.Key].FilePath);
                }
            }

            IEnumerable<string> intersection = lib1.Files.Keys.Except(lib2.Files.Keys);
            IterateDifferenceBetweenDictionaries(intersection, lib1, "", patch.filesToDelete);

            IEnumerable<string> inter = lib2.Files.Keys.Except(lib1.Files.Keys);
            IterateDifferenceBetweenDictionaries(inter, lib2, path_OldStandAlone, patch.filesToReplace);

            return patch;
        }

        private static void IterateDifferenceBetweenDictionaries(IEnumerable<string> enumerable, MD5Lib files, string path, Dictionary<string, string> where)
        {
            foreach (string item in enumerable)
            {
                where.Add(files.Files[item].FilePath, path + item);
            }
        }

        public static async void PatchGenerate(PatchInfo sb, string PatchPath, Action<string> OnComplete = null)
        {
            using (StreamWriter PatchWriter = new StreamWriter(PatchPath + "/patch.txt"))
            {
                await PatchWriter.WriteLineAsync(JsonConvert.SerializeObject(sb));
            }
            OnComplete?.Invoke("Patching");
        }
    }
}