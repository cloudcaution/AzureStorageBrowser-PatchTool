using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UnityEngine;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/11/2019
// Description: AzureRestUpdate contains all required methods and save them by string name in enum. 
//              During runtime, create delegate functions for each string name to link to actual methods
// Notes:       Due to concern of performance issues, I end up re-writing these codes spliting 
//              to AzureRestRequestAPI.cs and AzureRestPatchAPI.cs. This AzureRestUpdate might be a reference to them. 
//////////////////////////////////////

namespace iMVR_Launcher
{
    /// <summary>
    /// This is Older version of updating Azure API, currently not using this version any more
    /// But I wants to keep this just in case of misunderstand of newer updating Azure API.
    /// </summary>
    public class AzureRestUpdate : AzureRestAPI
    {
        /// <summary>
        /// url of current selected request method
        /// </summary>
        private string url;

        /// <summary>
        /// full url of current web request
        /// </summary>
        public string Url
        {
            get
            {
                return AzureRestClient.UrlEndpoint + url;
            }
            set
            {
                url = value;
            }
        }

        public string Oldversion;
        public string NewVersion;
        public string Md5;
        public int Size;
        public string latestVersion;

        /// <summary>
        /// type of request methods
        /// </summary>
        public RequestMethod requestMethod;


        public AzureRestUpdate(RequestMethod requestMethod, HttpVerb httpMethod,string oldV, string newV, string md5, int size)
        {
            switch (requestMethod)
            {
                case RequestMethod.CheckUpdate:
                    Url = AzureRestClient.CheckUpdateEndpoint;
                    break;
                case RequestMethod.AddPatchToDatabase:
                    Url = AzureRestClient.AddPatchEndpoint;
                    break;
                case RequestMethod.UpdateLatestClientVersion:
                    Url = AzureRestClient.updateLatestClientVersion;
                    latestVersion = newV;
                    break;
                case RequestMethod.DeleteClientPatchData:
                    url = AzureRestClient.deleteClientPatchData;
                    break;
                default:
                    break;
            }
            this.httpMethod = httpMethod;
            Oldversion = oldV;
            NewVersion = newV;
            Md5 = md5;
            Size = size;
        }

        /// <summary>
        /// enum of request method for AzureRestLogin
        /// </summary>
        public enum RequestMethod
        {
            CheckUpdate, AddPatchToDatabase, DeleteClientPatchData, UpdateLatestClientVersion, GetLatestClientVersion
        }
        /// <summary>
        /// check user input according to different request method
        /// </summary>
        /// <returns></returns>
        private bool RequestMethodValidation()
        {
            switch (requestMethod)
            {
                case RequestMethod.CheckUpdate:
                    Url = AzureRestClient.CheckUpdateEndpoint;
                    return true;
                default:
                    return false;
            }
        }
        #region Request
        /// <summary>
        /// Create a web request according to RequestType, if reqeustType is not initialRequest,
        /// set request method to 'GET' in order to make a pre-checking request.
        /// </summary>
        /// <param name="EndPoint">Url of object location</param>
        /// <param name="type">Request type</param>
        public void Request(string EndPoint, RequestMethod type)
        {
            WebRequest request = RequestBase(EndPoint, httpMethod.ToString());

            try
            {
                requestToGO = (RequestToGo)Delegate.CreateDelegate(typeof(RequestToGo), this, type.ToString());
                Task.Run(() => ParseResponse(requestToGO, request, type));
            }
            catch (ArgumentException ex)
            {
                Debug.Log(ex.Message + "Failed to match delegate function.\nPlease follow format of delegate function. " +
                    "Rules: public async Task<string> " + type.ToString() + "(WebRequest request)");
            }
        }

        /// <summary>
        /// show response message in pop up window for different status code.
        /// </summary>
        /// <param name="requestTo">request delegate function</param>
        /// <param name="request">web request</param>
        /// <param name="type">current request method</param>
        protected void ParseResponse(RequestToGo requestTo, WebRequest request, RequestMethod type)
        {
            string response = "";
            response = requestToGO(request).Result;
            try
            {
                ResponseMessage responseFromServer = JsonConvert.DeserializeObject<ResponseMessage>(response);
                if (responseFromServer.StatusCode == 200)
                    Debug.Log("Web request sent Successfully! " + responseFromServer.Message);
                else
                    Debug.Log(ErrorMessageFilter(responseFromServer));
            }
            catch (JsonReaderException ex)
            {
                Debug.Log("response cannot be serialized. " + response + ". Error: " + ex.Message + "\nUnexpected error occurs in the server, please try again later.");
            }
            catch (ArgumentNullException ex)
            {
                Debug.Log("Receieve no response from server. " + ex + "\nNo response from server, please check out the Internet.");
            }
        }
        #endregion

        #region Error Message check
        /// <summary>
        /// return different errorMessage depended on current request method
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private string ErrorMessageFilter(ResponseMessage response)
        {
            switch (requestMethod)
            {
                case RequestMethod.CheckUpdate:
                    AzureRestClient.downloadURL = response.DownloadURL;
                    AzureRestClient._needDownload = true;
                    return response.Message + "\nLatest verision is " + response.LatestVersion + "\npatch name is " + response.azureBlob + "\ncontainer is " + response.azureContainer;
                case RequestMethod.AddPatchToDatabase:
                    return "Unknown returned error message!" + response.Message + ";" + response.ErrorMessage;
                default:
                    return response.ErrorMessage;
            }
        }
        #endregion

        /// <summary>
        /// Initialize version check web request to remote server
        /// </summary>
        /// <param name="request">Web Request</param>
        /// <returns>return response message</returns>
        public async Task<string> CheckUpdate(WebRequest request)
        {
            return await SendRequest(request, "CheckingUpdate");
        }

        public async Task<string> AddPatchToDatabase(WebRequest request)
        {
            return await SendRequest(request, "AddToDataBase");
        }

        public async Task<string> DeleteClientPatchData(WebRequest request)
        {
            return await SendRequest(request, "DeletingClientPatchData");
        }

        public async Task<string> UpdateLatestClientVersion(WebRequest request)
        {
            return await SendRequest(request, "UpdatingLatestClientVersion");
        }

        #region Delegate methods for different requests
        /// <summary>
        /// <para>Initialize the connection with remote server.</para>
        /// <para>Rules: </para>
        /// <para>POST EndPoint (can be found in AzureRestClient)</para>
        /// <para>Header: Authorization, Bearer Access Token</para>
        /// <para>        ContentType, application/json</para>
        /// <para>Body:   serialized json data</para>
        /// </summary>
        /// <param name="request">Initialized web request</param>
        protected async Task<string> SendRequest(WebRequest request, string type)
        {
            try
            {
                if (httpMethod != HttpVerb.GET)
                {
                    request.ContentType = "application/json";
                    using (StreamWriter jsonPayload = new StreamWriter(request.GetRequestStream()))
                    {
                        // Create delegate function according to current request method and http method.
                        requestSendMethod = (RequestSendMethod)Delegate.CreateDelegate(typeof(RequestSendMethod), this, type.ToString());

                        // write body json string to request stream returning from delegate function created above
                        jsonPayload.Write(requestSendMethod());
                    }
                }
                return await WaitForResponse(request);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                    Debug.Log("<color=#ff2d2d>Server not responding, please try again later and check out your Internet.</color>");
                else
                    Debug.Log("<color=#ff2d2d>" + ex.Message + "</color>");
                return null;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return null;
            }
        }
        /// <summary>
        /// serialization class for checking update
        /// </summary>
        /// <returns></returns>
        protected string CheckingUpdate()
        {
            CheckUpdateInfo checkUpdateInfo = new CheckUpdateInfo
            {
                version = AzureRestClient.version
            };

            return JsonConvert.SerializeObject(checkUpdateInfo);
        }

        protected string UpdatingLatestClientVersion()
        {
            UpdateLatestClientVersionInfo updateInfo = new UpdateLatestClientVersionInfo()
            {
                latestClientVersion = latestVersion
            };

            return JsonConvert.SerializeObject(updateInfo);
        }

        protected string DeletingClientPatchData()
        {
            DeleteClientPatchDataInfo deleteClientPatch = new DeleteClientPatchDataInfo()
            {
                oldVersion = Oldversion,
                newVersion = NewVersion
            };

            return JsonConvert.SerializeObject(deleteClientPatch);
        }

        protected string AddToDataBase()
        {
            AddPatchToDatabaseInfo addPatch = new AddPatchToDatabaseInfo
            {
                oldVersion = Oldversion,
                newVersion = NewVersion,
                md5 = Md5,
                size = Size
            };
            return JsonConvert.SerializeObject(addPatch);
        }
        #endregion
    }

    ///// <summary>
    ///// class info of checkupdate
    ///// </summary>
    //internal class CheckUpdateInfo
    //{
    //    public string version;
    //}

    //internal class AddPatchToDatabaseInfo
    //{
    //    public string oldVersion { get; set; }
    //    public string newVersion { get; set; }
    //    public string md5 { get; set; }
    //    public int size { get; set; }
    //}

    //internal class DeleteClientPatchDataInfo
    //{
    //    public string oldVersion { get; set; }
    //    public string newVersion { get; set; }
    //}

    //internal class UpdateLatestClientVersionInfo
    //{
    //    public string latestClientVersion { get; set; }
    //}
}