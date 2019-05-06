using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using System.IO.Compression;
using Microsoft.WindowsAzure.Storage;
using iMVR_Launcher;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
// Description: Unity Editor window for making patch
//////////////////////////////////////
[ExecuteInEditMode]
[InitializeOnLoad]
public class Md5sumCollector : EditorWindow {

#if UNITY_EDITOR

    private bool AzureBlobBool = true;
    private bool AzureDatabaseBool = true;
    private int activeTab;
    private float progressStat = 0;
    private string path_OldStandAlone = "Old standalone";
    private string path_NewStandAlone = "newer standalone";
    private string path_OutputPatchFiles = "";
    private string path_ZipPatchFiles = "";
    private string PatchPath;
    private string versionOfStandaloneOne = "0.0";
    private string versionOfStandaloneSecond = "0.0";
    private string Name_BlobDirectory;
    private string progressBarName;
    private string Azure_Delete_OldVersion = "x.x";
    private string Azure_Delete_NewerVersion = "x.x";
    private string Azure_Latestes_Version;
    private string warningMessage = "";
    private string name_Container = "default";
    private string name_deletePatch_Container = "patch-#.#-#.#.zip";
    private string App_Name = "default";
    private string extension = ".zip";
    private string[] files_inOldStandAlone;
    private string[] files_inNewStandAlone;
    private string[] directory_inOldStandAlone;
    private string[] directory_inNewStandAlone;
    private string[] tabs = new string[] { "Generate Patch", "Output Patch", "Upload Patch" };
    private PatchInfo info;
    private Vector2 windowSize;
    private GUISkin guiskin;
    private AzureStorageAPI storageAPI;
    private bool toggle;

    private string Name_ZipPatchForUpload
    {
        get
        {
            return App_Name + "-patch-" + versionOfStandaloneOne + "-" + versionOfStandaloneSecond + extension;
        }
    }
    private string Name_ZipPatchForCompression
    {
        get
        {
            return (App_Name + "-patch-" + versionOfStandaloneOne + "-" + versionOfStandaloneSecond) + extension;
        }
    }  

    [MenuItem("Window/Patch Generator")]
    static void Init()
    {
        Md5sumCollector window = (Md5sumCollector)EditorWindow.GetWindow(typeof(Md5sumCollector), false, "PatchMaker");
        window.Show();
    }

    private void OnEnable()
    {
        PatchPath = Application.dataPath + "/Patches";
        AzureStorageManager.storageAccount = CloudStorageAccount.Parse(AzureStorageManager.connectionString);
        AzureStorageManager.blobClient = AzureStorageManager.storageAccount.CreateCloudBlobClient();
        windowSize = new Vector2(300, 400);
        guiskin = (GUISkin)Resources.Load("UISkin");
        storageAPI = new AzureStorageAPI(OnErrorMsg);
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        EditorStyles.whiteBoldLabel.wordWrap = true;
        this.minSize = windowSize;
        GUI.contentColor = Color.blue;
        GUILayout.Label("Developer Tool to make and test a patch", EditorStyles.whiteBoldLabel);
        GUI.contentColor = Color.white;
        activeTab = GUILayout.Toolbar(activeTab, tabs);
        if (activeTab == 0)
            PatchGenerate();
        else if (activeTab == 1)
            OutPutPatch();
        else
            UploadToServer();
         
        using (new EditorGUILayout.VerticalScope("Window")){  GUI.color = Color.red; EditorGUILayout.LabelField(warningMessage, EditorStyles.whiteBoldLabel); GUI.color = Color.white;}
        using (new EditorGUILayout.VerticalScope("Box"))
        {
            GUI.contentColor = Color.red;
            GUILayout.Label("Global Progress Bar", EditorStyles.boldLabel);
            Rect rect = EditorGUILayout.BeginVertical();
            EditorGUI.ProgressBar(rect, progressStat, progressBarName + "(" + progressStat * 100 + "%)");
            GUILayout.Space(18);
            EditorGUILayout.EndVertical();
        }
    }

    private void PatchGenerate()
    {
        using (new EditorGUILayout.VerticalScope("Box"))
        {
            GUILayout.Label("Path of older standalone folder");
            using (new EditorGUILayout.HorizontalScope("Box"))
            {
                if (GUILayout.Button(new GUIContent("Select folder", "Please select the directory of older standalone")))
                {
                    SelectFolder(out path_OldStandAlone, out files_inOldStandAlone, out directory_inOldStandAlone, out versionOfStandaloneOne);
                }
                path_OldStandAlone = EditorGUILayout.TextField("", path_OldStandAlone, GUILayout.MinWidth(30));
            }
        }

        using (new EditorGUILayout.VerticalScope("Box"))
        {
            GUILayout.Label("path of newer standalone folder", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Select folder", "Please select the directory of newer standalone")))
                {
                    SelectFolder(out path_NewStandAlone, out files_inNewStandAlone, out directory_inNewStandAlone, out versionOfStandaloneSecond);
                }
                path_NewStandAlone = EditorGUILayout.TextField("", path_NewStandAlone, GUILayout.MinWidth(30));
            }
        }

        using (new EditorGUILayout.VerticalScope("Box"))
        {
            GUILayout.Label("Generate Patch files list", EditorStyles.boldLabel);
            versionOfStandaloneOne = EditorGUILayout.TextField("Old Version: ", versionOfStandaloneOne);
            versionOfStandaloneSecond = EditorGUILayout.TextField("New Version: ", versionOfStandaloneSecond);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Generate", "Generate a patch log from given two standalone.")))
                {
                    Task.Run(() => PatchLogGenerator.GeneratePatchFromTwoStandAlone(files_inOldStandAlone, files_inNewStandAlone, 
                        directory_inOldStandAlone, directory_inNewStandAlone, Name_ZipPatchForUpload, path_OldStandAlone, path_NewStandAlone, ref progressStat, PatchPath, OnComplete));
                    progressBarName = "Making Patch...";
                }

                if (GUILayout.Button(new GUIContent("Open Patch Log", "Open the patch log made by 'Generate' button")))
                {
                    System.Diagnostics.Process.Start(PatchPath + @"\patch.txt");
                }
            }
            using (new EditorGUILayout.VerticalScope("Box"))
            {
                GUI.color = Color.red;
                if (GUILayout.Button(new GUIContent("Install Patch to Old Standalone", "Warning!!!\nPlease make backup of old standalone files\nApplying Patch will override old standalone!")))
                {
                    PopUpWarning();
                }
                GUI.color = Color.white;
            }
        }
    }

    private void OutPutPatch()
    {
        using (new EditorGUILayout.VerticalScope("Box"))
        {
            GUILayout.Label("Output Path", EditorStyles.boldLabel);
            EditorGUILayout.TextField("", path_OutputPatchFiles);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Select output folder", "Please select the directory where to save the patch files according to client patch log(named 'Patch.txt')")))
                {
                    path_OutputPatchFiles = EditorUtility.OpenFolderPanel("Load Folders", "", "");
                }

                if (GUILayout.Button(new GUIContent("Output and compress", "Output patch files to the folder selected above, and compress to zip.")))
                {
                    Task.Run(() => FileHelper.Export(path_OutputPatchFiles, info, PatchPath, path_NewStandAlone, path_OldStandAlone,
                        path_OutputPatchFiles, Name_ZipPatchForCompression, OnInitialize, OnUpdate, OnComplete));
                }
            }
        }
    }

    private void UploadToServer()
    {
        using (new EditorGUILayout.VerticalScope("Box"))
        {
            AzureBlobBool = EditorGUILayout.Foldout(AzureBlobBool, "AzureBlobStorage Implements", EditorStyles.foldout);
            if (AzureBlobBool)
            {
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    GUILayout.Label("Create a container on Cloud", EditorStyles.boldLabel);
                    name_Container = EditorGUILayout.TextField("Name of container", name_Container);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(new GUIContent("Create", "Create a remote cloud folder by given name in the textfield")))
                        {
                            if (!string.IsNullOrEmpty(name_Container))
                                Task.Run(() => storageAPI.CreateFolderOnCloud(name_Container, CheckOnComplete));
                            else
                                warningMessage = "Folder name cannot be empty!";
                        }
                        if (GUILayout.Button(new GUIContent("Delete", "Delete a remote cloud folder by given name in 'Name of container'")))
                            Task.Run(() => storageAPI.DeleteCloudDirectoryByName(name_Container, CheckOnComplete));
                    }
                }

                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    GUILayout.Label("Path of zipped file", EditorStyles.boldLabel);
                    EditorGUILayout.TextField("", path_ZipPatchFiles);
                    if (GUILayout.Button(new GUIContent("Select zipped patch file", "Please select the zipped patch file to upload")))
                    {
                        try
                        {
                            path_ZipPatchFiles = EditorUtility.OpenFilePanel("Load zip file", Directory.GetParent(path_OutputPatchFiles).ToString(), "");
                        }
                        catch (Exception)
                        {
                            path_ZipPatchFiles = EditorUtility.OpenFilePanel("Load zip file", "", "");
                        }
                    }

                    GUILayout.Label("Uploading file", EditorStyles.boldLabel);
                    using (new EditorGUILayout.HorizontalScope("Box"))
                    {
                        GUILayout.Label(new GUIContent("Patch version: " + versionOfStandaloneOne + "-" + versionOfStandaloneSecond, "Patch version : (old standalone version)-(new standalone version). Please go back to Generate path to change value if you want"));
                    }
                    
                    EditorGUILayout.LabelField("File name to upload: ", EditorStyles.boldLabel);
                    using (new EditorGUILayout.HorizontalScope("Box"))
                    {
                        EditorGUILayout.LabelField(new GUIContent((!string.IsNullOrEmpty(Name_BlobDirectory) ? Name_BlobDirectory + "/" : "") + Name_ZipPatchForUpload, "Filename consists of 'virtual direcotry' + '/' + 'patch-' + 'old version' + 'newer version' + '.zip'"));
                    }

                    Name_BlobDirectory = EditorGUILayout.TextField(new GUIContent("Name virtual directory:", "virtual directory in Azure blob storage"), Name_BlobDirectory);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(new GUIContent("Upload", "upload selected zip file to remote cloud storage.")))
                                Task.Run(() => storageAPI.UploadFileProgressively(name_Container, (!string.IsNullOrEmpty(Name_BlobDirectory) ? Name_BlobDirectory + "/" : "") + Name_ZipPatchForUpload, path_ZipPatchFiles, 2 * 1024 * 1024,
                                    versionOfStandaloneOne, versionOfStandaloneSecond, CheckOncomplete, OnComplete));
                        if (GUILayout.Button(new GUIContent("Delete", "Delete zipped file\nFilename => File name to upload\nContainer name => Name of container")))
                            Task.Run(() => storageAPI.DeleteCloudFileByName(name_Container, Name_ZipPatchForUpload, CheckOnComplete));
                    }
                }
            }
        }
        using (new EditorGUILayout.VerticalScope("Box"))
        {
            AzureDatabaseBool = EditorGUILayout.Foldout(AzureDatabaseBool, "AzureDatabase Implements", EditorStyles.foldout);
            if (AzureDatabaseBool)
            {
                GUILayout.Label("This Part is disabled since I removed links to database.");
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    toggle = GUILayout.Toggle(toggle, "Still want to see the content?");
                    if (toggle)
                    {
                        GUILayout.Label("Delete existed patch record on dataBase", EditorStyles.boldLabel);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Label("patch# from:", GUILayout.Width(75));
                            Azure_Delete_OldVersion = EditorGUILayout.TextField("", Azure_Delete_OldVersion, GUILayout.Width(40));
                            GUILayout.Label("to", GUILayout.Width(15));
                            Azure_Delete_NewerVersion = EditorGUILayout.TextField("", Azure_Delete_NewerVersion, GUILayout.Width(40));
                            if (GUILayout.Button(new GUIContent("Delete", "Delete a patch record that is on cloud")))
                            {
                                if (!string.IsNullOrEmpty(name_deletePatch_Container) && name_deletePatch_Container != "patch-.#-#.#.zip")
                                    Task.Run(() => storageAPI.DeletePatchInfoOnDataBase(AzureRestAPI.HttpVerb.POST, Azure_Delete_OldVersion, Azure_Delete_NewerVersion));
                                else
                                    warningMessage = "Please input a patch name";
                            }

                        }

                        GUILayout.Label("Update Latest Version On DataBase:", EditorStyles.boldLabel);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Label(new GUIContent("Version#: ", "This is for comparing local version # and server latest version #\nPlease enter version on x.x format(e.g. 1.0) where x is a number"), GUILayout.Width(60));
                            Azure_Latestes_Version = EditorGUILayout.TextField(Azure_Latestes_Version, GUILayout.Width(50));
                            if (GUILayout.Button(new GUIContent("Update", "Update the version number of latest patch")))
                            {
                                Task.Run(() => storageAPI.UpdatePatchVersion(AzureRestAPI.HttpVerb.POST, Azure_Latestes_Version));
                            }
                        }
                    }
                }

            }
        }
    }

    private void CheckOnComplete(bool result, string succeedwarning, string failedwarning)
    {
        if (result)
            warningMessage = succeedwarning;
        else
            warningMessage = failedwarning;
    }

    private void CheckOncomplete(double percent, string name)
    {
        progressBarName = name;
        progressStat = (float)percent;
    }

    private void CheckOncomplete(string name)
    {
        OnComplete(name);
    }

    private void SelectFolder(out string path, out string[] files, out string[] directories, out string version)
    {
        path = "";
        files = null;
        directories = null;
        version = "";
        try
        {
            path = EditorUtility.OpenFolderPanel("Patch Generator", "", "");
            UpdateSelectedFolderInfo(path, out files, out directories, out version);
        }
        catch(Exception ex)
        {
            warningMessage = "Path cannot be empty. Please select one existed folder! \nException message: " + ex.Message;
        }
    }

    private void UpdateSelectedFolderInfo(string path, out string[] files, out string[] directories, out string version)
    {
        files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
        
        // TODO: This is hardcode! I assumed the correct excutable file is always at index 0, later need to figure out a efficient way to find exactly the excutable file we need
        version = FileVersionInfo.GetVersionInfo(Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories)[0]).ProductVersion;
    }

#endif

    #region updating locally (For testing patch between selected newer standalone folder and older standalone standalone)
    public void PopUpWarning()
    {
        WindowPopUp window = (WindowPopUp)EditorWindow.GetWindow(typeof(WindowPopUp), false, "Pop-Up");
        Rect pos = window.position;
        pos.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
        window.position = pos;
        window.md5sum = this;
        window.ShowPopup();
    }

    public async void Patching()
    {
        float counter = OnInitialize("Updating...");
        info = JsonConvert.DeserializeObject<PatchInfo>(File.ReadAllText(PatchPath + "/patch.txt"));
        int total = info.filesToAdd.Count + info.filesToDelete.Count + info.filesToReplace.Count;
        await ApplyAllPatches(counter, total);
        OnComplete("Update");
        string tmp;
        UpdateSelectedFolderInfo(path_OldStandAlone, out files_inOldStandAlone, out directory_inOldStandAlone, out tmp);
    }

    private async Task ApplyAllPatches(float counter, int total)
    {
        counter = await ApplyPatch(counter, total, info.filesToReplace, FileHelper.UpdateOldFiles);
        counter = await ApplyPatch(counter, total, info.filesToAdd, FileHelper.UpdateOldFiles);
        ApplyPatch(counter, total, info.filesToDelete, FileHelper.DeleteOldFiles);
    }

    private async Task<float> ApplyPatch(float counter, int total, Dictionary<string, string> info, Func<string,string, Task> MethodToFile)
    {
        foreach (KeyValuePair<string, string> item in info)
        {
            await MethodToFile(item.Key, item.Value);
            counter = OnUpdate(counter, total);
        }
        return counter;
    }

    private void ApplyPatch(float counter, int total, Dictionary<string, string> info, Action<string> MethodToFile)
    {
        foreach (KeyValuePair<string, string> item in info)
        {
            MethodToFile(item.Key);
            counter = OnUpdate(counter, total);
        }
    }

    private float OnInitialize(string str)
    {
        progressBarName = str;
        progressStat = 0f;
        return 0;
    }

    private float OnUpdate(float counter, int total)
    {
        progressStat = counter / total;
        return counter + 1;
    }

    private void OnComplete(string str)
    {
        progressBarName = str + " Complete.";
        progressStat = 1f;
        warningMessage = str + " Complete.";
        activeTab = Mathf.Clamp(activeTab + 1, 0, 2);
    }

    #endregion
    private void OnErrorMsg(string str)
    {
        warningMessage = str;
    }
}
