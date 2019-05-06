using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
// Description: The difference between this and AzureRestUpdate is AzureRestPatchAPI does not need to
//              create delegate functions at runtime, all needed info are given by a serialized string in parameter.
//////////////////////////////////////

namespace iMVR_Launcher
{
    public class AzureRestPatchAPI
    {
        private int _size;
        private string endPoint;
        private string contentBody;
        private string currentVersion;
        private string olderVersion;
        private string newerVersion;
        private string _md5;
        private string container;
        private string blob;
        private AzureRestRequestAPI requestAPI = new AzureRestRequestAPI();
        private AzureRestAPI.HttpVerb method;
        private ResponseMessage response;

        public AzureRestPatchAPI(string url, AzureRestAPI.HttpVerb httpMethod)
        {
            endPoint = url;
            method = httpMethod;   
        }

        public void Initialize(string url, AzureRestAPI.HttpVerb httpMethod)
        {
            endPoint = url;
            method = httpMethod;
        }

        #region Customized Methods
        public string CheckServerVersion()
        {
            return SendRequest(CheckingUpdate());
        }

        public string UploadingPatch(string olderVersion, string newerVersion, string md5, int size, string containerName, string blobName)
        {
            this.olderVersion = olderVersion;
            this.newerVersion = newerVersion;
            _md5 = md5;
            _size = size;
            container = containerName;
            blob = blobName;

            return SendRequest(AddToDataBase());
        }

        public string UpdateLatestVersionOnDataBase(string latestVersion)
        {
            newerVersion = latestVersion;

            return SendRequest(UpdatingLatestClientVersion());
        }

        public string DeletePatchFileOnServer(string older, string newer)
        {
            olderVersion = older;
            newerVersion = newer;

            return SendRequest(DeletingClientPatchData());
        }
        #endregion

        #region web API methods
        private string SendRequest(string ContentBody)
        {
            return ParseResponseMessage(requestAPI.Request(endPoint, method, ContentBody));
        }

        private string SendRequest(string ContentBody, string accessToken)
        {
            return ParseResponseMessage(requestAPI.Request(endPoint, method, ContentBody, accessToken));
        }
        #endregion

        #region web response implements
        // TODO now this is just assuming we only use message or errorMessage, need to add more later
        private string ParseResponseMessage(ResponseMessage response)
        {
            if (response.StatusCode != 200)
                return response.ErrorMessage;
            else
                return response.Message;
        }
        #endregion

        /// <summary>
        /// serialization class for checking update
        /// </summary>
        /// <returns></returns>
        protected string CheckingUpdate()
        {
            CheckUpdateInfo checkUpdateInfo = new CheckUpdateInfo
            {
                version = currentVersion
            };

            return JsonConvert.SerializeObject(checkUpdateInfo);
        }

        protected string UpdatingLatestClientVersion()
        {
            UpdateLatestClientVersionInfo updateInfo = new UpdateLatestClientVersionInfo()
            {
                latestClientVersion = newerVersion
            };

            return JsonConvert.SerializeObject(updateInfo);
        }

        protected string DeletingClientPatchData()
        {
            DeleteClientPatchDataInfo deleteClientPatch = new DeleteClientPatchDataInfo()
            {
                oldVersion = olderVersion,
                newVersion = newerVersion
            };

            return JsonConvert.SerializeObject(deleteClientPatch);
        }

        protected string AddToDataBase()
        {
            AddPatchToDatabaseInfo addPatch = new AddPatchToDatabaseInfo
            {
                oldVersion = olderVersion,
                newVersion = newerVersion,
                md5 = _md5,
                size = _size,
                azureContainer = container,
                azureBlob = blob
            };
            return JsonConvert.SerializeObject(addPatch);
        }
    }

    /// <summary>
    /// class info of checkupdate
    /// </summary>
    internal class CheckUpdateInfo
    {
        public string version;
    }

    internal class AddPatchToDatabaseInfo
    {
        public string oldVersion { get; set; }
        public string newVersion { get; set; }
        public string md5 { get; set; }
        public int size { get; set; }
        public string azureContainer { get; set; }
        public string azureBlob { get; set; }
    }

    internal class DeleteClientPatchDataInfo
    {
        public string oldVersion { get; set; }
        public string newVersion { get; set; }
    }

    internal class UpdateLatestClientVersionInfo
    {
        public string latestClientVersion { get; set; }
    }

    public class ResponseMessage
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
        public int SoftwareKeyRemainingActivationTimes { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; }
        public string LatestVersion { get; set; }
        public string DownloadURL { get; set; }
        public string md5 { get; set; }
        public string azureContainer { get; set; }
        public string azureBlob { get; set; }
    }

}
