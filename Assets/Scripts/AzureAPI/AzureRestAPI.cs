using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Security.Claims;
using System.IO;
using System.Net.Mime;
using Newtonsoft.Json;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net;
using System.Data;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
// Description: Base Class for customed HTTP web request.
//////////////////////////////////////

namespace iMVR_Launcher
{
    /// <summary>
    /// Base class for all Rest API usage
    /// </summary>
    public class AzureRestAPI
    {
        /// <summary>
        /// Http method enum
        /// </summary>
        public enum HttpVerb { GET, POST, PUT, DELETE };

        protected RequestSendMethod requestSendMethod;
        protected delegate string RequestSendMethod();

        /// <summary>
        /// Http web request
        /// </summary>
        public WebRequest request;

        /// <summary>
        /// Http web response
        /// </summary>
        public WebResponse response;

        private int timeout = 5;

        /// <summary>
        /// Remote resource url of object location
        /// </summary>
        public string ResourceEndpoint;

        /// <summary>
        /// Delegate function to make different request
        /// </summary>
        protected RequestToGo requestToGO;

        /// <summary>
        /// Initialize a delegate to make web request
        /// </summary>
        /// <param name="request">Web request</param>
        protected delegate Task<string> RequestToGo(WebRequest request);

        /// <summary>
        /// Http method
        /// </summary>
        protected HttpVerb httpMethod;

        public int Timeout
        {
            get
            {
                return timeout * 1000;
            }
        }

        /// <summary>
        /// Set cancel flag
        /// </summary>
        protected void Cancel()
        {
            //AzureRestClient.Cancel = true;
        }

        #region request functions
        /// <summary>
        /// Create a webrequest according to Endpoint
        /// </summary>
        /// <param name="EndPoint">It is url of object location</param>
        /// <returns>Return a web request</returns>
        protected WebRequest RequestBase(string EndPoint)
        {
            WebRequest request = WebRequest.Create(EndPoint);
            request.Method = httpMethod.ToString();

            return request;
        }

        /// <summary>
        /// Create a webrequest according to Endpoint and http method
        /// </summary>
        /// <param name="EndPoint">Url of object location</param>
        /// <param name="method">Web request method</param>
        /// <returns>Return a web request</returns>
        protected WebRequest RequestBase(string EndPoint, string method)
        {
            WebRequest request = WebRequest.Create(EndPoint);
            request.Method = method;
            request.Timeout = Timeout;

            return request;
        }

        /// <summary>
        /// Create a web request
        /// </summary>
        /// <param name="EndPoint">Url of object location</param>
        /// <returns>Return a web request</returns>
        protected WebRequest Request(string EndPoint)
        {
            ResourceEndpoint = EndPoint;
            return RequestBase(EndPoint);
        }
        #endregion

        /// <summary>
        /// Add access Token to current request header
        /// </summary>
        /// <param name="request">Current web request</param>
        /// <returns>return back the web request</returns>
        protected WebRequest AddAccessHeader(WebRequest request, string accessToken)
        {
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            return request;
        }

        /// <summary>
        /// abstract class for later override depended on usage.
        /// </summary>
        /// <param name="request"></param>
        protected async virtual void ButtonClick(WebRequest request)
        {
            try
            {
                request.ContentType = "application/json";

                if (httpMethod != HttpVerb.GET)
                {
                    using (StreamWriter jsonPayload = new StreamWriter(request.GetRequestStream()))
                    {

                    }
                }
                await WaitForResponse(request);
            }
            catch (WebException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Get Response From Server
        /// <summary>
        /// Wait for a web response 
        /// </summary>
        /// <param name="request">current web request</param>
        /// <returns>response result</returns>
        protected async Task<string> WaitForResponse(WebRequest request)
        {
            return await Response(request);
        }

        /// <summary>
        /// If nothing goes wrong, read message from response body, otherwise read error message.
        /// </summary>
        /// <param name="request">Current web request</param>
        /// <returns>Response message</returns>
        protected async Task<string> Response(WebRequest request)
        {
            string responseValue = string.Empty;
            try
            {
                response = request.GetResponse();
                responseValue = await ReadResponse(response, false);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                    throw ex;
                responseValue = await ReadResponse(ex.Response, true);
            }
            catch (Exception ex)
            {
                throw;
            }

            return responseValue;
        }

        /// <summary>
        /// Using stream to read response stream. deserialize message if get error response
        /// </summary>
        /// <param name="response">Web Response</param>
        /// <param name="isError">Error flag</param>
        /// <returns>Response message</returns>
        protected async Task<string> ReadResponse(WebResponse response, bool isError)
        {
            string responseValue = string.Empty;
            using (Stream responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        responseValue = await reader.ReadToEndAsync();
                        HttpWebResponse httpWeb = response as HttpWebResponse;
                        Debug.Log(httpWeb.StatusCode + ":" + responseValue);
                        if (String.IsNullOrEmpty(responseValue))
                            responseValue = httpWeb.StatusCode.ToString();
                    }
                }
            }
            return responseValue;
        }
        #endregion
    }
}
