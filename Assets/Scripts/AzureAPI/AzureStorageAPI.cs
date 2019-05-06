using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 4/27/2019
// Description: Wrap up all related web api together in one class for easier use.
//              Update: (5/3/2019) remove APIs that call database.
//////////////////////////////////////
namespace iMVR_Launcher
{
    /// <summary>
    /// Azure blob storage wrapper class.
    /// Initialize storage account and client when enabled,
    /// set up complete and progressOnchange value for each storage web call
    /// </summary>
    public class AzureStorageAPI
    {
        /// <summary>
        /// Error Message printer from Md5sumCollector class
        /// </summary>
        private Action<string> OnErrorMsg;

        /// <summary>
        /// Contructor to initialize storage and blob client 
        /// </summary>
        /// <param name="OnErrorMsg">Error Message callback function</param>
        public AzureStorageAPI(Action<string> OnErrorMsg)
        {
            AzureStorageManager.storageAccount = CloudStorageAccount.Parse(AzureStorageManager.connectionString);
            AzureStorageManager.blobClient = AzureStorageManager.storageAccount.CreateCloudBlobClient();
            this.OnErrorMsg = OnErrorMsg;
        }

        #region Azure blob storage methods
        /// <summary>
        /// Create a container by give name with callback when complete
        /// </summary>
        /// <param name="name_Container">name of container</param>
        /// <param name="onComplete">Error Message callback function</param>
        public async void CreateFolderOnCloud(string name_Container, Action<bool, string, string> onComplete)
        {
            try
            {
                await AzureStorageManager.CreateContainer(name_Container,
                            (bool result) => onComplete(result, "Create folder " + name_Container + " successfully.", "Failed to create folder " + name_Container));
            }
            catch (Exception ex)
            {
                PrintError("Name of container is not specified. Error message: " + ex.Message);
            }
        }

        /// <summary>
        /// Upload blob file with progress report callback and progress oncomplete callback
        /// </summary>
        /// <param name="name_Container">Name of Container</param>
        /// <param name="Name_ZipPatchForUpload">Name of blob file</param>
        /// <param name="path_ZipPatchFiles">Path of blob file on local disk</param>
        /// <param name="blockSize">Size of datablock for uploading every excute time</param>
        /// <param name="versionOne">Version number of old patch number</param>
        /// <param name="versionTwo">Version number of latest patch number</param>
        /// <param name="onProgress">Upload Onprogress callback</param>
        /// <param name="OnComplete">Upload OnComplete callback</param>
        public async void UploadFileProgressively(string name_Container, string Name_ZipPatchForUpload, string path_ZipPatchFiles, int blockSize,
            string versionOne, string versionTwo, Action<double, string> onProgress, Action<string> OnComplete)
        {
            try
            {
                await AzureStorageManager.UploadFromFileProgressively(name_Container, Name_ZipPatchForUpload, path_ZipPatchFiles, blockSize,
                (double percent) => onProgress(percent, "Uploading"),
                () => {
                    OnComplete("Uploading");
                    Debug.Log("Trying to update info on database, but links to database had been removed.");
                    //AzureRestPatchAPI patchAPI = new AzureRestPatchAPI(AzureRestClient.UrlEndpoint + AzureRestClient.AddPatchEndpoint, AzureRestAPI.HttpVerb.POST);
                    //patchAPI.UploadingPatch(versionOne, versionTwo, FileHelper.FindMD5(path_ZipPatchFiles), GetSizeOfFile(path_ZipPatchFiles), name_Container, Name_ZipPatchForUpload);
                });
            }
            catch (Exception ex)
            {
                PrintError("Name of container or Path of zip file is invalid. Error message: " + ex.Message);
            }
        }

        /// <summary>
        /// Upload blob file with progress report callback and progress oncomplete callback but without updating database
        /// </summary>
        /// <param name="name_Container">Name of Container</param>
        /// <param name="Name_ZipPatchForUpload">Name of blob file</param>
        /// <param name="path_ZipPatchFiles">Path of blob file on local disk</param>
        /// <param name="blockSize">Size of datablock for uploading every excute time</param>
        /// <param name="onProgress">Upload Onprogress callback</param>
        /// <param name="OnComplete">Upload OnComplete callback</param>
        public async void UploadFileProgressivelyWithoutUpDate(string name_Container, string Name_ZipPatchForUpload, string path_ZipPatchFiles, int blockSize,
            Action<double, string> onProgress, Action<string> OnComplete)
        {
            try
            {
                await AzureStorageManager.UploadFromFileProgressively(name_Container, Name_ZipPatchForUpload, path_ZipPatchFiles, blockSize,
                (double percent) => onProgress(percent, "Uploading"),
                () => {
                    OnComplete("Uploading");
                });
            }
            catch (Exception ex)
            {
                PrintError("Name of container or Path of zip file is invalid. Error Message: " + ex.Message);  
            }
        }

        /// <summary>
        /// HTTP request to update latest version in database
        /// </summary>
        /// <param name="httpMethod">HTTP request method</param>
        /// <param name="Azure_Delete_NewerVersion">The latest version number</param>
        public void UpdatePatchVersion(AzureRestAPI.HttpVerb httpMethod, string Azure_Delete_NewerVersion)
        {
            Debug.Log("Trying to connect to database, but the links to databases had been removed.");
            //I remove the connection to database, so this method should be disabled.
            //AzureRestPatchAPI patchAPI = new AzureRestPatchAPI(AzureRestClient.UrlEndpoint + AzureRestClient.updateLatestClientVersion, httpMethod);
            //patchAPI.UpdateLatestVersionOnDataBase(Azure_Delete_NewerVersion);
        }

        /// <summary>
        /// Get size of patch file
        /// </summary>
        /// <param name="filePath">Path of the patch file</param>
        /// <returns></returns>
        private int GetSizeOfFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return (int)fileInfo.Length;
        }

        /// <summary>
        /// Delete blob file on Azure storage
        /// </summary>
        /// <param name="name_Container">Name of container</param>
        /// <param name="Name_ZipPatch">Name of zipped patch file</param>
        /// <param name="onComplete">Delete OnComplete callback</param>
        public async void DeleteCloudFileByName(string name_Container, string Name_ZipPatch, Action<bool, string, string> onComplete)
        {
            try
            {
                await AzureStorageManager.DeleteBlob(name_Container, Name_ZipPatch,
                (bool result) => onComplete(result, "Successfully delete file " + Name_ZipPatch, "Failed to delete file " + Name_ZipPatch));
            }
            catch (Exception ex)
            {
                PrintError("Name of file is not specified. Error message: " + ex.Message);
            }
        }

        /// <summary>
        /// Delete container on Azure Storage
        /// </summary>
        /// <param name="name_Container">Name of container</param>
        /// <param name="onComplete">Delete OnComplete callback</param>
        public async void DeleteCloudDirectoryByName(string name_Container, Action<bool, string, string> onComplete)
        {
            try
            {
                await AzureStorageManager.DeleteContainer(name_Container,
                (bool result) => onComplete(result, "Successfully delete directory " + name_Container, "Failed to delete directory " + name_Container));
            }
            catch (Exception ex)
            {
                PrintError("Name of container is not specified. Error message: " + ex.Message);
            }
        }

        /// <summary>
        /// Delete patch record on Database table. Patch record contains { olderVersion-newerVersion }
        /// </summary>
        /// <param name="method">HTTP request method</param>
        /// <param name="Azure_Delete_OldVersion">The older version of Patch record</param>
        /// <param name="Azure_Delete_NewerVersion">The newer version of Patch record</param>
        public void DeletePatchInfoOnDataBase(AzureRestAPI.HttpVerb method, string Azure_Delete_OldVersion, string Azure_Delete_NewerVersion)
        {
            Debug.Log("Trying to conncet to database, but the links to database had been removed.");
            ////I remove the connection to database, so this method should be disabled.
            //AzureRestPatchAPI azureRestPatch = new AzureRestPatchAPI(AzureRestClient.UrlEndpoint + AzureRestClient.deleteClientPatchData, AzureRestAPI.HttpVerb.POST);
            //azureRestPatch.DeletePatchFileOnServer(Azure_Delete_OldVersion, Azure_Delete_NewerVersion);
        }
        #endregion

        /// <summary>
        /// Error message callback function
        /// </summary>
        /// <param name="error"></param>
        private void PrintError(string error)
        {
            OnErrorMsg?.Invoke(error);
            Debug.Log(error);
        }
    }

    /// <summary>
    /// Serializing model of FilesInfo 
    /// </summary>
    public struct FilesInfo
    {
        public string FilePath;
        public string FileMD5;
    }

    /// <summary>
    /// MD5 and name container, storing MD5 of the file and its file name
    /// </summary>
    public class MD5Lib
    {
        public Dictionary<string, FilesInfo> Files;
        public MD5Lib()
        {
            Files = new Dictionary<string, FilesInfo>();
        }
    }

    /// <summary>
    /// The infomation that stored in Patch manifest file, which is called "Patch.txt" for now
    /// </summary>
    public class PatchInfo
    {
        public string patchversion;
        public Dictionary<string, string> filesToReplace;
        public Dictionary<string, string> filesToAdd;
        public Dictionary<string, string> filesToDelete;

        public PatchInfo()
        {
            filesToReplace = new Dictionary<string, string>();
            filesToAdd = new Dictionary<string, string>();
            filesToDelete = new Dictionary<string, string>();
        }
    }
}
