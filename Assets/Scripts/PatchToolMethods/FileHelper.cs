using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
// Description: This is File helper class, that contains method to find MD5 of files, creates or remove files, 
//              or generates a patch according to instructions of patch log
//////////////////////////////////////
namespace iMVR_Launcher
{
    public static class FileHelper 
    {
        private static WaitForSecondsRealtime waitOne = new WaitForSecondsRealtime(1);
        public static string FindMD5(string filename)
        {
            try
            {
                using (MD5 md5 = MD5.Create())
                {
                    using (FileStream stream = File.OpenRead(filename))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
            return "";
        }

        public static async Task UpdateOldFiles(string path1, string path2)
        {
            string tmp = "";
            try
            {
                if (File.Exists(path1))
                {
                    using (FileStream reader = new FileStream(path1, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        if (!File.Exists(path2))
                        {
                            if (!Directory.Exists(tmp = Path.GetDirectoryName(path2)))
                                Directory.CreateDirectory(tmp);
                        }
                        using (StreamWriter writer = new StreamWriter(path2, false))
                        {
                            await reader.CopyToAsync(writer.BaseStream);
                        }
                    }
                }
                else
                    Directory.CreateDirectory(path2);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log("This message is showing up in case, could be ignored. " + ex.Message + ";" + path1 + " xx " + path2);
            }
        }

        public static void DeleteOldFiles(string path)
        {
            try
            {
                System.IO.File.Delete(path);
            }
            catch (Exception ex)
            {
                try
                {
                    System.IO.Directory.Delete(path, true);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log("The current file already been deleted " + e);
                }
            }
        }

        #region Export files
        public static async void Export(string str, PatchInfo info, string PatchPath, string path_NewStandAlone, 
            string path_OldStandAlone, string path_OutputPatchFiles, string NameOfCompressionZip,
            Func<string, float> OnInitialize,
            Func<float, int, float> OnUpdate, Action<string> OnComplete)
        {
            float counter = OnInitialize("Exporting...");
            info = JsonConvert.DeserializeObject<PatchInfo>(File.ReadAllText(PatchPath + "/patch.txt"));
            string tmpPath = "";
            string currentPath = "";
            int total = info.filesToAdd.Count + info.filesToReplace.Count;
            try
            {
                foreach (var item in info.filesToReplace)
                {
                    currentPath = str + item.Key.Substring(path_NewStandAlone.Length);
                    using (FileStream reader = new FileStream(item.Key, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        if (!Directory.Exists(tmpPath = Path.GetDirectoryName(currentPath)))
                            Directory.CreateDirectory(tmpPath);

                        using (FileStream writer = new FileStream(currentPath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            await reader.CopyToAsync(writer);
                        }
                    }
                    counter = OnUpdate(counter, total);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex.HelpLink + " " + ex.Message);
            }

            //OnComplete("Exporting");
            GenerateClientPatch(str, PatchPath, info, path_NewStandAlone, path_OldStandAlone, path_OutputPatchFiles, NameOfCompressionZip, OnInitialize, OnComplete);
        }

        private static async void GenerateClientPatch(string str, string PatchPath, PatchInfo info, string path_NewStandAlone, 
            string path_OldStandAlone, string path_OutputPatchFiles, string Name_ZipPatchForCompression, Func<string, float> OnUpdate = null, Action<string> OnComplete = null)
        {
            try
            {
                info = JsonConvert.DeserializeObject<PatchInfo>(File.ReadAllText(PatchPath + "/patch.txt"));
                PatchInfo clientInfo = new PatchInfo();

                foreach (KeyValuePair<string, string> item in info.filesToAdd)
                {
                    clientInfo.filesToAdd.Add(item.Key.Substring(path_NewStandAlone.Length), item.Value.Substring(path_OldStandAlone.Length));
                }

                foreach (KeyValuePair<string, string> item in info.filesToDelete)
                {
                    clientInfo.filesToDelete.Add(item.Key.Substring(path_OldStandAlone.Length), "");
                }

                foreach (KeyValuePair<string, string> item in info.filesToReplace)
                {
                    clientInfo.filesToReplace.Add(item.Key.Substring(path_NewStandAlone.Length), item.Value.Substring(path_OldStandAlone.Length));
                }

                using (StreamWriter PatchWriter = new StreamWriter(path_OutputPatchFiles + "/patch.txt"))
                {
                    await PatchWriter.WriteLineAsync(JsonConvert.SerializeObject(clientInfo));
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex.Message);
            }

            // TODO: Need to track progress of Compressing Patch
            OnUpdate?.Invoke("Cant track Compression progress, just wait.");
            CompressPatchFolder(str, path_OutputPatchFiles, Name_ZipPatchForCompression);
            OnComplete?.Invoke("Done Compression");
        }

        private static void CompressPatchFolder(string path, string path_OutputPatchFiles, string Name_ZipPatchForCompression)
        {
            ZipFile.CreateFromDirectory(path_OutputPatchFiles, path + "/../" + Name_ZipPatchForCompression, System.IO.Compression.CompressionLevel.Optimal, true);
        }
        #endregion
    }
}